# Master-Install.ps1
# Instalador Idempotente, Seguro y Profesional para DAM-Suite.

$InstallRoot = "C:\ProgramData\DAM-Suite"
$ApiPort = 7155
$AppPool = "DAM_Pool"
$WebSite = "DAM.Api"
$SvcName = "DAM_Worker"

# --- 1. Verificaciones de Entorno ---
function Confirm-Environment {
    $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        Write-Host "❌ ERROR: Ejecuta este script como Administrador." -ForegroundColor Red; exit
    }
    Write-Host "💻 Estación: $($env:COMPUTERNAME) | Usuario: $($env:USERNAME)" -ForegroundColor Gray
}

# --- 2. Gestión de IIS (Punto 5) ---
function Setup-IIS-Feature {
    Write-Progress -Activity "Instalando DAM" -Status "Verificando IIS..." -PercentComplete 10
    $feature = Get-WindowsOptionalFeature -Online -FeatureName "IIS-WebServerRole"
    if ($feature.State -ne "Enabled") {
        Write-Host "`n📢 Requisito: IIS no está habilitado." -ForegroundColor Yellow
        $ans = Read-Host "¿Desea activar IIS y componentes .NET ahora? (S/N)"
        if ($ans -eq 's' -or $ans -eq 'S') {
            Write-Host "⏳ Habilitando componentes de Windows (puede tardar)..." -ForegroundColor Cyan
            Enable-WindowsOptionalFeature -Online -FeatureName "IIS-WebServerRole", "IIS-ASPNET45", "IIS-DefaultDocument", "IIS-ISAPIExtensions" -All -NoRestart
        } else { Write-Host "❌ Instalación cancelada por el usuario."; exit }
    }
}

# --- 3. Ejecución de Despliegue (Idempotente) ---
function Invoke-Deployment {
    # 3.1 Estructura (Single Source of Truth)
    $Paths = @("App\Api", "App\Service", "Data", "Logs")
    foreach ($p in $Paths) { 
        $full = Join-Path $InstallRoot $p
        if (!(Test-Path $full)) { New-Item $full -ItemType Directory -Force | Out-Null }
    }

    # 3.2 Permisos de Escritura para SQLite y Logs
    $Acl = Get-Acl $InstallRoot
    $Rule = New-Object System.Security.AccessControl.FileSystemAccessRule("Users", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $Acl.SetAccessRule($Rule)
    Set-Acl $InstallRoot $Acl

    # 3.3 Copia de Archivos
    Write-Progress -Activity "Instalando DAM" -Status "Copiando binarios..." -PercentComplete 40
    $BaseDir = Split-Path $PSCommandPath
    Copy-Item "$BaseDir\DAM.Api\*" -Destination "$InstallRoot\App\Api" -Recurse -Force
    Copy-Item "$BaseDir\DAM.Host.WindowsService\*" -Destination "$InstallRoot\App\Service" -Recurse -Force

    # 3.4 Configuración IIS
    Write-Progress -Activity "Instalando DAM" -Status "Configurando IIS..." -PercentComplete 70
    Import-Module WebAdministration
    if (!(Test-Path "IIS:\AppPools\$AppPool")) { New-WebAppPool $AppPool }
    Set-ItemProperty "IIS:\AppPools\$AppPool" -Name "managedRuntimeVersion" -Value "" # .NET Core

    if (!(Test-Path "IIS:\Sites\$WebSite")) {
        New-Website -Name $WebSite -Port $ApiPort -PhysicalPath "$InstallRoot\App\Api" -ApplicationPool $AppPool
    } else {
        Set-ItemProperty "IIS:\Sites\$WebSite" -Name "physicalPath" -Value "$InstallRoot\App\Api"
    }

    # 3.5 Configuración Windows Service
    Write-Progress -Activity "Instalando DAM" -Status "Configurando Servicio..." -PercentComplete 90
    $BinPath = "$InstallRoot\App\Service\DAM.Host.WindowsService.exe"
    
    if (Get-Service $SvcName -ErrorAction SilentlyContinue) {
        Stop-Service $SvcName -Force -ErrorAction SilentlyContinue
        & sc.exe config $SvcName binPath= $BinPath | Out-Null
    } else {
        New-Service -Name $SvcName -BinaryPathName $BinPath -DisplayName "DAM Monitoring Service" -StartupType Automatic | Out-Null
    }
    Start-Service $SvcName
}

# --- Flujo de Ejecución ---
try {
    Confirm-Environment
    Setup-IIS-Feature
    Invoke-Deployment
    Write-Progress -Activity "Instalando DAM" -Status "¡Listo!" -PercentComplete 100
    Write-Host "`n✅ INSTALACIÓN EXITOSA" -ForegroundColor Green
    Write-Host "📍 Ubicación: $InstallRoot"
    Write-Host "🌐 API URL: http://localhost:$ApiPort" -ForegroundColor Cyan
}
catch {
    Write-Host "`n❌ Error en la instalación: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    Write-Progress -Activity "Instalando DAM" -Completed
    Write-Host "`nPresione cualquier tecla para salir..."
    $null = [Console]::ReadKey()
}