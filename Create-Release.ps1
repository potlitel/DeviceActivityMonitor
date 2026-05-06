# Create-Release.ps1
# Script de empaquetado profesional con transformacion dinamica y UX mejorada.

$SolutionDir = Split-Path -Parent $PSCommandPath
$ReleaseDir = Join-Path -Path $SolutionDir -ChildPath "Releases"
$BuildStamp = (Get-Date).ToString('yyyyMMdd-HHmmss')
$InstallRoot = "C:\ProgramData\DAM-Suite" 
$DbPath = "$InstallRoot\Data\DAM_Database.db"

# Proyectos a procesar
$Projects = @(
    @{ Name = "DAM.Host.WindowsService"; Path = "$SolutionDir\DAM.Host.WindowsService\DAM.Host.WindowsService.csproj" },
    @{ Name = "DAM.Api"; Path = "$SolutionDir\DAM.Api\DAM.Api.csproj" }
)

# --- Funciones de Transformacion Inteligente ---
function Transform-AppSettings {
    param ([string]$Path, [string]$ProjectName)
    
    if (Test-Path $Path) {
        try {
            $config = Get-Content $Path -Raw | ConvertFrom-Json
            $changed = $false

            # 1. Transformacion de ConnectionStrings
            if ($config.ConnectionStrings) {
                if ($config.ConnectionStrings.DefaultConnection) {
                    $config.ConnectionStrings.DefaultConnection = "Data Source=$DbPath"
                    $changed = $true
                }
                if ($config.ConnectionStrings.SQLiteConnection) {
                    $config.ConnectionStrings.SQLiteConnection = "Data Source=$DbPath"
                    $changed = $true
                }
            }

            # 2. Transformacion de Serilog (Rutas de Log)
            if ($config.Serilog.WriteTo) {
                foreach ($t in $config.Serilog.WriteTo) {
                    if ($t.Args -and $t.Args.path) {
                        $t.Args.path = "$InstallRoot\Logs\$ProjectName-log.txt"
                        $changed = $true
                    }
                }
            }

            if ($changed) {
                $config | ConvertTo-Json -Depth 32 | Set-Content $Path -Encoding UTF8
                Write-Host "   [CONFIG] AppSettings de '$ProjectName' actualizado." -ForegroundColor DarkGray
            }
        }
        catch {
            Write-Warning "No se pudo transformar ${Path}: $($_.Exception.Message)"
        }
    }
}

# --- Proceso Principal ---
try {
    Write-Host "Iniciando Generacion de Release: $BuildStamp" -ForegroundColor Cyan
    $Staging = Join-Path $env:TEMP "DAM_Build_$BuildStamp"
    New-Item $Staging -ItemType Directory -Force | Out-Null

    $i = 0
    foreach ($Proj in $Projects) {
        $i++
        $percent = ($i / $Projects.Count) * 80
        Write-Progress -Activity "Compilando Suite DAM" -Status "Procesando: $($Proj.Name)" -PercentComplete $percent

        $TargetFolder = Join-Path $Staging $Proj.Name
        Write-Host "Publicando $($Proj.Name)..." -ForegroundColor Gray
        
        # DAM.Api no puede usar single-file (requerido por IIS)
        $IsApi = $Proj.Name -eq "DAM.Api"
        $SingleFile = if ($IsApi) { "" } else { "-p:PublishSingleFile=true" }
        $PublishArgs = @("publish", $Proj.Path, "-c", "Release", "-r", "win-x64", "--self-contained", "true", "-o", $TargetFolder, "--nologo")
        if ($SingleFile) { $PublishArgs += $SingleFile }
        
        & dotnet @PublishArgs
        
        if ($LASTEXITCODE -ne 0) { throw "Error publicando $($Proj.Name)" }
        
        Write-Progress -Activity "Compilando Suite DAM" -Status "Configurando '$($Proj.Name)' para produccion" -PercentComplete $percent
        Transform-AppSettings -Path (Join-Path $TargetFolder "appsettings.json") -ProjectName $Proj.Name
    }

    # Copiar Master-Install -> Install-Suite
    $MasterScript = Join-Path $SolutionDir "Master-Install.ps1"
    if (Test-Path $MasterScript) {
        Copy-Item $MasterScript -Destination (Join-Path $Staging "Install-Suite.ps1") -Force
        Write-Host "  Install-Suite.ps1 inyectado." -ForegroundColor Gray
    }

    # Copiar carpeta Installers para despliegue offline
    $InstallersSrc = Join-Path $SolutionDir "Installers"
    if (Test-Path $InstallersSrc) {
        $InstallersDest = Join-Path $Staging "Installers"
        New-Item $InstallersDest -ItemType Directory -Force | Out-Null
        Copy-Item "$InstallersSrc\*" -Destination $InstallersDest -Recurse -Force
        $count = (Get-ChildItem $InstallersDest).Count
        Write-Host "  Installers offline inyectados ($count archivo(s))." -ForegroundColor Gray
    } else {
        Write-Host "  ADVERTENCIA: Carpeta Installers no encontrada. Ejecute Download-Installers.ps1 primero." -ForegroundColor Yellow
    }

    # Generar Readme.txt con instrucciones
    $ReadmeContent = @"
================================================================================
     DAM-Suite | Instalacion Offline
     Version: $BuildStamp
================================================================================

REQUISITOS PREVIOS:
  - Windows 10/11 o Windows Server 2019+
  - Ejecutar como Administrador

INSTALACION:
  1. Descomprima este ZIP en una carpeta temporal
  2. Abra PowerShell como Administrador
  3. Navegue a la carpeta descomprimida
  4. Ejecute: .\Install-Suite.ps1
  5. El script instalara automaticamente:
     - IIS (si no esta habilitado)
     - ASP.NET Core Hosting Bundle (desde installers offline incluidos)
     - API en http://localhost:7155
     - Servicio Windows: DAM Monitoring Service

DESTINO DE INSTALACION:
  C:\ProgramData\DAM-Suite

ESTRUCTURA TRAS INSTALACION:
  App\Api\        -> Archivos de la API REST
  App\Service\    -> Servicio de monitoreo Windows
  Data\           -> Base de datos SQLite
  Logs\           -> Registros de la aplicacion

SOPORTE:
  Reporte issues o contacte al equipo de desarrollo.
================================================================================
"@

    $ReadmePath = Join-Path $Staging "Readme.txt"
    Set-Content -Path $ReadmePath -Value $ReadmeContent -Encoding UTF8
    Write-Host "  Readme.txt generado." -ForegroundColor Gray

    # Empaquetado Final
    Write-Progress -Activity "Compilando Suite DAM" -Status "Creando paquete ZIP final..." -PercentComplete 95
    if (!(Test-Path $ReleaseDir)) { New-Item $ReleaseDir -ItemType Directory }
    $ZipPath = Join-Path $ReleaseDir "DAM_Release_$BuildStamp.zip"
    Compress-Archive -Path "$Staging\*" -DestinationPath $ZipPath -Force

    # Limpieza y Retencion (Mantener ultimos 5)
    Remove-Item $Staging -Recurse -Force
    $Old = Get-ChildItem $ReleaseDir -Filter "*.zip" | Sort-Object LastWriteTime -Descending
    if ($Old.Count -gt 5) { $Old | Select-Object -Skip 5 | Remove-Item -Force }

    Write-Host "`nRELEASE LISTO: $ZipPath" -ForegroundColor Green
}
catch {
    Write-Host "`nERROR CRITICO: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally { Write-Progress -Activity "Compilando Suite DAM" -Completed }
