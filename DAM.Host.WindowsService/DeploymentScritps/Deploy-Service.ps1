<#
.SYNOPSIS
    Script de Despliegue y Configuración del Servicio de Windows "DeviceActivityMonitor".

.DESCRIPTION
    Este script automatiza la detención, desinstalación, publicación, instalación, 
    configuración de recuperación y el inicio del servicio de Windows.
    Requiere permisos de Administrador para ejecutarse.

.PARAMETER ProjectPath
    Ruta al directorio del proyecto .NET Core (DAM.Host.WindowsService.csproj).
    
.PARAMETER DeployPath
    Ruta donde se copiarán los archivos publicados y desde donde se ejecutará el servicio.

.PARAMETER ServiceName
    Nombre del servicio de Windows a crear. Por defecto: DeviceActivityMonitor.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectPath,
    
    [Parameter(Mandatory=$true)]
    [string]$DeployPath,
    
    [string]$ServiceName = "DeviceActivityMonitor"
)

# --- Funciones Auxiliares ---

# Función para dibujar una barra de progreso simulada en la consola
function Show-Progress {
    param(
        [string]$Message,
        # Tiempo en segundos que durará la simulación de progreso
        [int]$Seconds = 2
    )
    $MaxSteps = 20
    Write-Host "" # Nueva línea para mejor formato
    Write-Host "$Message" -ForegroundColor Cyan
    
    # Bucle para dibujar la barra
    for ($i = 1; $i -le $MaxSteps; $i++) {
        $ProgressBar = "[" + ("=" * $i) + (" " * ($MaxSteps - $i)) + "]"
        # El caracter `r (retorno de carro) mueve el cursor al inicio de la línea
        Write-Host "`r$ProgressBar Procesando..." -NoNewline -ForegroundColor Green
        # Implementa el 'sleep' de .NET Core para simular el progreso
        Start-Sleep -Milliseconds ([int](($Seconds * 1000) / $MaxSteps))
    }
    Write-Host " [OK]" -ForegroundColor Green
}

function StopAndUninstallService {
    param(
        [string]$Name
    )
    if (Get-Service -Name $Name -ErrorAction SilentlyContinue) {
        Show-Progress -Message "⚙️ Deteniendo servicio existente '$Name'..." -Seconds 2
        Stop-Service -Name $Name -Force -ErrorAction SilentlyContinue
        
        Show-Progress -Message "🧹 Desinstalando servicio existente '$Name'..." -Seconds 2
        # Utiliza sc.exe delete para desinstalar
        & sc.exe delete $Name | Out-Null
        
        # Esperar un momento para que el sistema libere los recursos
        Start-Sleep -Seconds 1
    }
}

# --- 1. Verificación de Permisos y Entorno ---
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "Este script DEBE ejecutarse con permisos de Administrador."
    exit 1
}

$ExecutableName = "DAM.Host.WindowsService.exe"
$TargetExecutable = "$DeployPath\$ExecutableName"
$ServiceDisplayName = "Device Activity Monitor (DAM)"

Write-Host "========================================================" -ForegroundColor Yellow
Write-Host " Iniciando el Despliegue del Servicio '$ServiceName'" -ForegroundColor Yellow
Write-Host "========================================================" -ForegroundColor Yellow


# --- 2. Gestión de Servicio Existente ---
Show-Progress -Message "🔍 Verificando estado del servicio existente..." -Seconds 1
StopAndUninstallService -Name $ServiceName

# --- 3. Publicación del Proyecto .NET Core (Self-Contained) ---
Show-Progress -Message "📦 Publicando proyecto .NET (Self-Contained, Single-File)..." -Seconds 5
# Usamos un bloque try/catch para manejo de errores más claro en PowerShell
try {
    # 2>&1 asegura que la salida normal y de error se capturen en $PublishResult
    $PublishResult = & dotnet publish $ProjectPath -c Release -r win-x64 --self-contained true -o $DeployPath -p:PublishSingleFile=true 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "La publicación de .NET falló. Revise la salida:"
        
        # 💡 SOLUCIÓN: Usar el operador -join para convertir el array ($PublishResult)
        # en una única cadena, donde cada línea está separada por un salto de línea (`n`)
        $ErrorMessage = $PublishResult -join "`n" 
        Write-Error $ErrorMessage # <-- Cambiado
        exit 1
    }
} catch {
    Write-Error "Error grave al ejecutar 'dotnet publish': $($_.Exception.Message)"
    exit 1
}

# --- 4. Instalación del Servicio ---
Show-Progress -Message "💾 Instalando servicio '$ServiceName'..." -Seconds 3
$InstallResult = & sc.exe create $ServiceName binPath="`"$TargetExecutable`"" start="auto" DisplayName="$ServiceDisplayName" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error "La instalación del servicio falló. Resultado: $InstallResult"
    exit 1
}

# --- 5. Configuración de Recuperación (Auto-Reinicio Resiliente) ---
Show-Progress -Message "🛡️ Configurando acciones de recuperación (Auto-Reinicio)..." -Seconds 2
# reset= 0, actions= restart/1000/restart/1000/restart/1000
$FailureResult = & sc.exe failure $ServiceName reset= 0 actions= restart/1000/restart/1000/restart/1000 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Warning "Fallo al configurar la recuperación automática: $FailureResult"
}

# --- 6. Inicio del Servicio ---
Show-Progress -Message "▶️ Iniciando servicio '$ServiceName'..." -Seconds 3
Start-Service -Name $ServiceName -ErrorAction Stop

Write-Host ""
Write-Host "--------------------------------------------------------" -ForegroundColor Green
Write-Host "✅ Despliegue Completado y Servicio Iniciado Correctamente." -ForegroundColor Green
Write-Host "--------------------------------------------------------" -ForegroundColor Green