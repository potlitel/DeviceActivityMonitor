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
        Write-Host "ERROR: Ejecuta este script como Administrador." -ForegroundColor Red; exit
    }
    Write-Host "Estacion: $($env:COMPUTERNAME) | Usuario: $($env:USERNAME)" -ForegroundColor Gray
}

# --- 2. Instalación del Hosting Bundle (Offline) ---
function Install-HostingBundle {
    Write-Progress -Activity "Instalando DAM" -Status "Verificando ASP.NET Core Hosting Bundle..." -PercentComplete 15
    $modulePath = "C:\Windows\System32\inetsrv\aspnetcore.dll"
    $moduleV2Path = "C:\Windows\System32\inetsrv\aspnetcorev2.dll"

    if ((Test-Path $modulePath) -or (Test-Path $moduleV2Path)) {
        Write-Host "  [OK] ASP.NET Core Hosting Bundle detectado." -ForegroundColor Gray
        return
    }

    Write-Host ""
    Write-Host "Hosting Bundle no detectado. Instalando desde paquete offline..." -ForegroundColor Yellow

    $BaseDir = Split-Path $PSCommandPath
    $InstallerDir = Join-Path $BaseDir "Installers"
    $Installer = Get-ChildItem -Path $InstallerDir -Filter "dotnet-hosting-*-win.exe" | Sort-Object Name -Descending | Select-Object -First 1

    if (!$Installer) {
        Write-Host ""
        Write-Host "ERROR: Installer del Hosting Bundle no encontrado en el paquete." -ForegroundColor Red
        Write-Host "Ejecute Download-Installers.ps1 antes de crear el release." -ForegroundColor Yellow
        Write-Host ""
        exit
    }

    Write-Host "  Instalando: $($Installer.Name)" -ForegroundColor Gray
    $proc = Start-Process -FilePath $Installer.FullName -ArgumentList "/install", "/quiet", "/norestart" -Wait -PassThru -NoNewWindow
    if ($proc.ExitCode -ne 0) {
        Write-Host "ERROR: Fallo la instalacion del Hosting Bundle (ExitCode: $($proc.ExitCode))" -ForegroundColor Red
        exit
    }
    Write-Host "  Reiniciando IIS para registrar el modulo..." -ForegroundColor Gray
    & iisreset /restart | Out-Null
    Write-Host "  [OK] Hosting Bundle instalado y IIS reiniciado." -ForegroundColor Green
}

# --- 3. Gestión de IIS ---
function Setup-IIS-Feature {
    Write-Progress -Activity "Instalando DAM" -Status "Verificando IIS..." -PercentComplete 10
    $feature = Get-WindowsOptionalFeature -Online -FeatureName "IIS-WebServerRole"
    if ($feature.State -ne "Enabled") {
        Write-Host "`nREQUISITO: IIS no esta habilitado." -ForegroundColor Yellow
        $ans = Read-Host "¿Desea activar IIS y componentes ahora? (S/N)"
        if ($ans -eq 's' -or $ans -eq 'S') {
            Write-Host "Habilitando componentes de Windows (puede tardar)..." -ForegroundColor Cyan
            Enable-WindowsOptionalFeature -Online -FeatureName "IIS-WebServerRole", "IIS-ASPNET45", "IIS-DefaultDocument", "IIS-ISAPIExtensions" -All -NoRestart | Out-Null
        } else { Write-Host "ERROR: Instalacion cancelada por el usuario."; exit }
    }
}

# --- 4. Ejecución de Despliegue (Idempotente) ---
function Invoke-Deployment {
    # 4.1 Estructura
    $Paths = @("App\Api", "App\Service", "Data", "Logs")
    foreach ($p in $Paths) {
        $full = Join-Path $InstallRoot $p
        if (!(Test-Path $full)) { New-Item $full -ItemType Directory -Force | Out-Null }
    }

    # 4.2 Permisos
    $UsersSid = New-Object System.Security.Principal.SecurityIdentifier("S-1-5-32-545")
    $Acl = Get-Acl $InstallRoot
    $Rule = New-Object System.Security.AccessControl.FileSystemAccessRule($UsersSid, "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
    $Acl.SetAccessRule($Rule)
    Set-Acl $InstallRoot $Acl

    # 4.3 Detener componentes existentes para liberar archivos
    $svcExisted = Get-Service $SvcName -ErrorAction SilentlyContinue
    if ($svcExisted -and $svcExisted.Status -eq 'Running') {
        Write-Host "  Deteniendo servicio $SvcName..." -ForegroundColor Gray
        Stop-Service $SvcName -Force -ErrorAction SilentlyContinue
    }

    Import-Module WebAdministration | Out-Null
    if (Test-Path "IIS:\AppPools\$AppPool") {
        Write-Host "  Reciclando AppPool $AppPool..." -ForegroundColor Gray
        Restart-WebAppPool $AppPool -ErrorAction SilentlyContinue
    }

    # 4.4 Copia de Archivos (ahora sin bloqueo)
    Write-Progress -Activity "Instalando DAM" -Status "Copiando binarios..." -PercentComplete 40
    $BaseDir = Split-Path $PSCommandPath
    Copy-Item "$BaseDir\DAM.Api\*" -Destination "$InstallRoot\App\Api" -Recurse -Force
    Copy-Item "$BaseDir\DAM.Host.WindowsService\*" -Destination "$InstallRoot\App\Service" -Recurse -Force

    # 4.5 Configuración IIS
    Write-Progress -Activity "Instalando DAM" -Status "Configurando IIS..." -PercentComplete 70

    if (!(Test-Path "IIS:\AppPools\$AppPool")) {
        New-WebAppPool $AppPool | Out-Null
    }
    Set-ItemProperty "IIS:\AppPools\$AppPool" -Name "managedRuntimeVersion" -Value ""

    if (!(Test-Path "IIS:\Sites\$WebSite")) {
        New-Website -Name $WebSite -Port $ApiPort -PhysicalPath "$InstallRoot\App\Api" -ApplicationPool $AppPool | Out-Null
    } else {
        Set-ItemProperty "IIS:\Sites\$WebSite" -Name "physicalPath" -Value "$InstallRoot\App\Api"
    }

    # 4.6 Configuración Windows Service
    Write-Progress -Activity "Instalando DAM" -Status "Configurando Servicio..." -PercentComplete 90
    $BinPath = "$InstallRoot\App\Service\DAM.Host.WindowsService.exe"

    if ($svcExisted) {
        & sc.exe config $SvcName binPath= $BinPath | Out-Null
    } else {
        New-Service -Name $SvcName -BinaryPathName $BinPath -DisplayName "DAM Monitoring Service" -StartupType Automatic | Out-Null
    }
    Start-Service $SvcName | Out-Null

    return @{
        ServiceExisted = ($null -ne $svcExisted)
    }
}

# --- 5. Reporte Final ---
function Show-Summary {
    param ([bool]$ServiceExisted)

    Write-Host ""
    Write-Host "INSTALACION EXITOSA" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Gray

    # Estado del Servicio Windows
    $svc = Get-Service $SvcName -ErrorAction SilentlyContinue
    if ($svc) {
        $action = if ($ServiceExisted) { "Actualizado" } else { "Creado" }
        Write-Host "[$action] Servicio: $($svc.DisplayName)" -ForegroundColor Gray
        Write-Host "   Nombre    : $($svc.ServiceName)" -ForegroundColor Gray
        Write-Host "   Estado    : $($svc.Status)" -ForegroundColor Gray
        Write-Host "   Inicio    : $($svc.StartType)" -ForegroundColor Gray
    } else {
        Write-Host "  [ERROR] Servicio '$SvcName' no encontrado." -ForegroundColor Red
    }

    # Estado del Website IIS
    Import-Module WebAdministration | Out-Null
    $site = Get-Website $WebSite -ErrorAction SilentlyContinue
    if ($site) {
        $binding = $site.bindings.Collection[0]
        Write-Host "[OK] Sitio IIS: $($site.Name)" -ForegroundColor Gray
        Write-Host "   URL       : http://localhost:$ApiPort" -ForegroundColor Gray
        Write-Host "   Estado    : $($site.State)" -ForegroundColor Gray
        Write-Host "   AppPool   : $($site.applicationPool)" -ForegroundColor Gray
        Write-Host "   Path      : $($site.PhysicalPath)" -ForegroundColor Gray
    } else {
        Write-Host "  [ERROR] Sitio IIS '$WebSite' no encontrado." -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "Ubicacion : $InstallRoot" -ForegroundColor Gray
    Write-Host "API URL   : http://localhost:$ApiPort" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Gray
}

# --- Flujo de Ejecución ---
try {
    Confirm-Environment
    Setup-IIS-Feature
    Install-HostingBundle
    $result = Invoke-Deployment
    Write-Progress -Activity "Instalando DAM" -Status "Listo!" -PercentComplete 100
    Show-Summary -ServiceExisted $result.ServiceExisted
}
catch {
    Write-Host "`nERROR en la instalacion: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    Write-Progress -Activity "Instalando DAM" -Completed
    Write-Host "`nPresione cualquier tecla para salir..."
    $null = [Console]::ReadKey()
}
