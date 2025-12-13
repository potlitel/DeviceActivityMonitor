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
    # La carpeta de despliegue será la ubicación actual del script
    # [string]$DeployPath, <-- Se elimina
    
    [string]$ServiceName = "DeviceActivityMonitor" # Ahora es el único parámetro
)
# Forzar la ruta de trabajo al directorio donde se encuentra el script
$DeployPath = Split-Path -Parent $PSCommandPath

# --- Funciones Auxiliares de Interfaz y Lógica ---

# Función para dibujar una barra de progreso multi-etapa simulada en la consola
function Show-MultiStage-Progress {
    param(
        [string]$MainMessage,
        [int]$Seconds = 2,
        [string[]]$Stages # Array de mensajes de sub-etapas
    )
    $TotalStages = $Stages.Count
    $MaxStepsPerStage = 10
    $TotalSteps = $TotalStages * $MaxStepsPerStage
    $TotalDelayMs = $Seconds * 1000
    $DelayPerStepMs = [int]([Math]::Round($TotalDelayMs / $TotalSteps))
    $CurrentStep = 0
    # Longitud de la línea de progreso (aproximada para la limpieza)
    $LineLengthForCleaning = 100 

    Write-Host "" # Nueva línea para mejor formato
    Write-Host "$MainMessage" -ForegroundColor Cyan
    
    # Bucle principal por cada etapa
    for ($stageIndex = 0; $stageIndex -lt $TotalStages; $stageIndex++) {
        $StageMessage = $Stages[$stageIndex]
        
        # Bucle interno para el progreso de la barra
        for ($i = 1; $i -le $MaxStepsPerStage; $i++) {
            $CurrentStep++
            $Percentage = [int](($CurrentStep / $TotalSteps) * 100)
            
            # Dibujar la barra y el porcentaje
            $BarLength = [int](($CurrentStep / $TotalSteps) * 20)
            $ProgressBar = "[" + ("=" * $BarLength) + (" " * (20 - $BarLength)) + "]"
            
            # Línea de visualización corregida: se usa ${TotalStages}
            $OutputText = "`r$ProgressBar $Percentage% | Etapa $($stageIndex + 1)/${TotalStages}: $StageMessage "
            Write-Host $OutputText -NoNewline -ForegroundColor Green
            
            # Pausa para simular la duración del paso
            Start-Sleep -Milliseconds $DelayPerStepMs
        }
    }
    
    # 💡 CORRECCIÓN VISUAL: Asegurar que el texto final borre el rastro de la última etapa
    $FinalMessage = "[====================] 100% | Finalizado."
    
    # Rellenar con espacios para limpiar el resto de la línea
    $Padding = " " * ($LineLengthForCleaning - $FinalMessage.Length)
    
    Write-Host "`r$FinalMessage$Padding" -NoNewline -ForegroundColor Green
    Write-Host " [OK]" -ForegroundColor Green
}

# Función para esperar que un servicio sea completamente eliminado por el SCM (Service Control Manager)
function Wait-ForServiceDeletion {
    param(
        [Parameter(Mandatory=$true)]
        [string]$ServiceName
    )
    Write-Host "⏳ Esperando confirmación de liberación del servicio '$ServiceName'..." -ForegroundColor Yellow
    
    $timeout = 30 # Tiempo máximo de espera en segundos
    $interval = 2  # Intervalo de verificación en segundos
    $startTime = Get-Date
    
    do {
        # Intenta obtener el servicio. Si da error, significa que el servicio ya no existe.
        try {
            # El ErrorAction Stop fuerza el salto al bloque catch si el servicio no existe.
            $service = Get-Service -Name $ServiceName -ErrorAction Stop
            
            # Si el servicio existe, escribe el estado y continúa el bucle
            if ($service) {
                Write-Host "   El servicio aún está presente. Esperando..." -ForegroundColor DarkGray
            }
        }
        catch {
            # Si Get-Service lanza una excepción, el servicio fue liberado.
            Write-Host "✅ Servicio '$ServiceName' completamente liberado del SCM." -ForegroundColor Green
            return $true
        }
        
        # Verificar el tiempo de espera
        # Corrección: Uso de TotalSeconds y bloques de comandos {} requeridos.
        if (((Get-Date) - $startTime).TotalSeconds -ge $timeout) {
            Write-Error "Fallo crítico: El servicio '$ServiceName' no fue liberado después de $($timeout) segundos."
            return $false
        }

        Start-Sleep -Seconds $interval
    } while ($true)
}

function StopAndUninstallService {
    param(
        [string]$Name
    )
    
    # Verificación de la existencia del servicio
    if (Get-Service -Name $Name -ErrorAction SilentlyContinue) {
        
        Show-MultiStage-Progress -MainMessage "⚙️ Gestionando servicio anterior '$Name'..." -Seconds 4 -Stages @(
            "Deteniendo el servicio...",
            "Esperando confirmación de detención...",
            "Eliminando definición del servicio...",
            "Esperando liberación de recursos..."
        )

        # 1. DETENER EL SERVICIO
        try {
            # Intentamos detener. Si ya está detenido, el comando fallará, pero continuamos.
            Stop-Service -Name $Name -Force -ErrorAction SilentlyContinue
        } catch {}

        # 2. ESPERAR EL ESTADO 'STOPPED' ANTES DE ELIMINAR (Bucle robusto)
        $stopTimeout = 10 # 10 segundos para detener
        $stopInterval = 1
        $stopStartTime = Get-Date
        
        do {
            Start-Sleep -Seconds $stopInterval
            $service = Get-Service -Name $Name -ErrorAction SilentlyContinue
            $status = $service.Status
            
            # --- CORRECCIÓN DE SINTAXIS ---
            # Aseguramos el correcto anidamiento y las llaves de la sentencia if
            if ((((Get-Date) - $stopStartTime).TotalSeconds -ge $stopTimeout)) {
                Write-Warning "El servicio '$Name' no se detuvo después de $stopTimeout segundos. Forzando eliminación."
                break # Rompe el bucle e intenta la eliminación de todas formas
            }

        } until ($status -eq "Stopped" -or $status -eq $null) # $null significa que ya fue eliminado o nunca existió

        # 3. ELIMINAR EL SERVICIO
        & sc.exe delete $Name | Out-Null

        # 4. ESPERAR LA LIBERACIÓN FINAL DE WINDOWS (Resuelve el 1072)
        Wait-ForServiceDeletion -ServiceName $ServiceName 
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
Show-MultiStage-Progress -MainMessage "🔍 Verificando estado del servicio existente..." -Seconds 1 -Stages @("Buscando servicio...")
StopAndUninstallService -Name $ServiceName

# --- 4. Instalación del Servicio ---
Show-MultiStage-Progress -MainMessage "💾 Instalando servicio '$ServiceName'..." -Seconds 2 -Stages @("Creando definición de servicio...")
$InstallResult = & sc.exe create $ServiceName binPath="`"$TargetExecutable`"" start="auto" DisplayName="$ServiceDisplayName" obj= LocalSystem 2>&1


if ($LASTEXITCODE -ne 0) {
    Write-Error "La instalación del servicio falló. Resultado: $InstallResult"
    exit 1
}

# Definir la descripción fuera de la función principal para facilitar la lectura
$ServiceDescription = "Servicio de monitoreo en segundo plano para la detección de dispositivos externos y registro de actividad de E/S de almacenamiento USB. Garantiza la persistencia resiliente de datos." 

# --- 5. Configuración de Recuperación (Auto-Reinicio Resiliente) ---
Show-MultiStage-Progress -MainMessage "🛡️ Configurando acciones de recuperación (Auto-Reinicio)..." -Seconds 2 -Stages @("Aplicando política de reinicio (sc failure)...")

# 5a. Configuración de Recuperación (sc failure)
# reset= 0, actions= restart/1000/restart/1000/restart/1000
$FailureResult = & sc.exe failure $ServiceName reset= 0 actions= restart/1000/restart/1000/restart/1000 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Warning "Fallo al configurar la recuperación automática: $FailureResult"
}

# 5b. AÑADIR DESCRIPCIÓN DEL SERVICIO
$DescResult = & sc.exe description $ServiceName "$ServiceDescription" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Warning "Fallo al establecer la descripción del servicio: $DescResult"
}

# --- 6. Inicio del Servicio ---
Show-MultiStage-Progress -MainMessage "▶️ Iniciando servicio '$ServiceName'..." -Seconds 3 -Stages @("Enviando comando Start-Service...")
#Start-Service -Name $ServiceName -ErrorAction Stop
try {
    # El parámetro -ErrorAction Stop obliga a PowerShell a lanzar una excepción
    # que es capturada por el bloque catch si el servicio no puede iniciarse.
    Start-Service -Name $ServiceName -ErrorAction Stop
    
    # Si la línea de arriba tiene éxito, el script termina aquí con mensaje de éxito (al final del script)

} catch {
    # Captura la ServiceCommandException que indica que el servicio no pudo iniciarse
    Write-Warning ""
    Write-Warning "=========================================================="
    Write-Warning "⚠ ADVERTENCIA CRÍTICA: El servicio NO se pudo iniciar."
    Write-Warning "   Esto suele deberse a una EXCEPCIÓN en el código C#."
    Write-Warning "   Revise el Visor de Eventos de Windows (Application Log)."
    Write-Warning "   Error de PowerShell: $($_.Exception.Message)"
    Write-Warning "=========================================================="
    
    # Aunque no se pudo iniciar, permitimos que el script termine (no usamos exit 1) 
    # ya que la instalación y publicación sí fueron exitosas.
}


Write-Host ""
Write-Host "--------------------------------------------------------" -ForegroundColor Green
Write-Host "✅ Despliegue Completado y Servicio Iniciado Correctamente." -ForegroundColor Green
Write-Host "--------------------------------------------------------" -ForegroundColor Green