# Download-Installers.ps1
# Descarga los installers necesarios para despliegue offline de DAM-Suite.

$InstallerDir = Join-Path (Split-Path -Parent $PSCommandPath) "Installers"
if (!(Test-Path $InstallerDir)) { New-Item $InstallerDir -ItemType Directory -Force | Out-Null }

$Installers = @(
    @{
        Name = "dotnet-hosting-10.0.5-win.exe"
        Url  = "https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/10.0.5/dotnet-hosting-10.0.5-win.exe"
        Desc = "ASP.NET Core 10.0 Hosting Bundle (requerido para IIS)"
    }
)

Write-Host "Descargando installers para despliegue offline..." -ForegroundColor Cyan
Write-Host ""

$i = 0
foreach ($inst in $Installers) {
    $i++
    $dest = Join-Path $InstallerDir $inst.Name
    if (Test-Path $dest) {
        Write-Host "  [EXISTENTE] $($inst.Name)" -ForegroundColor Yellow
        continue
    }
    Write-Host "  [$i/$($Installers.Count)] Descargando: $($inst.Name)" -ForegroundColor Gray
    Write-Host "        Descripcion: $($inst.Desc)" -ForegroundColor DarkGray
    try {
        Invoke-WebRequest -Uri $inst.Url -OutFile $dest -UseBasicParsing
        $size = [math]::Round((Get-Item $dest).Length / 1MB, 2)
        Write-Host "        Completado ($size MB)" -ForegroundColor Green
    }
    catch {
        Write-Host "        ERROR: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "        Descargue manualmente desde: $($inst.Url)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Installers listos en: $InstallerDir" -ForegroundColor Green
