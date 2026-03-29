# # Create-Release.ps1
# # Script de empaquetado profesional con transformación dinámica de configuraciones.

# $SolutionDir = Split-Path -Parent $PSCommandPath
# $ReleaseDir = Join-Path -Path $SolutionDir -ChildPath "Releases"
# $BuildStamp = (Get-Date).ToString('yyyyMMdd-HHmmss')
# $InstallRoot = "C:\ProgramData\DAM-Suite" 
# $DbPath = "$InstallRoot\Data\DAM_Database.db"

# # Proyectos a procesar
# $Projects = @(
#     @{ Name = "DAM.Host.WindowsService"; Path = "$SolutionDir\DAM.Host.WindowsService\DAM.Host.WindowsService.csproj" },
#     @{ Name = "DAM.Api"; Path = "$SolutionDir\DAM.Api\DAM.Api.csproj" }
# )

# # --- Funciones de Transformación Inteligente ---
# function Transform-AppSettings {
#     param ([string]$Path, [string]$ProjectName)
    
#     if (Test-Path $Path) {
#         try {
#             $config = Get-Content $Path -Raw | ConvertFrom-Json
#             $changed = $false

#             # 1. Transformación de ConnectionStrings (Detecta múltiples nombres)
#             if ($config.ConnectionStrings) {
#                 # Caso API: DefaultConnection
#                 if ($config.ConnectionStrings.DefaultConnection) {
#                     $config.ConnectionStrings.DefaultConnection = "Data Source=$DbPath"
#                     $changed = $true
#                 }
#                 # Caso Worker: SQLiteConnection
#                 if ($config.ConnectionStrings.SQLiteConnection) {
#                     $config.ConnectionStrings.SQLiteConnection = "Data Source=$DbPath"
#                     $changed = $true
#                 }
#             }

#             # 2. Transformación de Serilog (Rutas de Log)
#             if ($config.Serilog.WriteTo) {
#                 foreach ($t in $config.Serilog.WriteTo) {
#                     if ($t.Args -and $t.Args.path) {
#                         $t.Args.path = "$InstallRoot\Logs\$ProjectName-log.txt"
#                         $changed = $true
#                     }
#                 }
#             }

#             if ($changed) {
#                 $config | ConvertTo-Json -Depth 32 | Set-Content $Path -Encoding UTF8
#                 Write-Host "   [CONFIG] AppSettings de '$ProjectName' actualizado correctamente." -ForegroundColor DarkGray
#             }
#         }
#         catch {
#             Write-Warning "No se pudo transformar ${Path}: $($_.Exception.Message)"
#         }
#     }
# }

# # --- Proceso Principal ---
# try {
#     Write-Host "🚀 Iniciando Generación de Release: $BuildStamp" -ForegroundColor Cyan
#     $Staging = Join-Path $env:TEMP "DAM_Build_$BuildStamp"
#     New-Item $Staging -ItemType Directory -Force | Out-Null

#     $i = 0
#     foreach ($Proj in $Projects) {
#         $i++
#         $percent = ($i / $Projects.Count) * 80
#         Write-Progress -Activity "Compilando Suite DAM" -Status "Procesando: $($Proj.Name)" -PercentComplete $percent

#         $TargetFolder = Join-Path $Staging $Proj.Name
#         Write-Host "📦 Publicando $($Proj.Name)..." -ForegroundColor Gray
        
#         # Publicación .NET
#         & dotnet publish $Proj.Path -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $TargetFolder --nologo
        
#         if ($LASTEXITCODE -ne 0) { throw "Error publicando $($Proj.Name)" }
        
#         # Inyectar configuración de producción detectando estructura
#         Transform-AppSettings -Path (Join-Path $TargetFolder "appsettings.json") -ProjectName $Proj.Name
#     }

#     # Copiar Script de Instalación (Master-Install -> Install-Suite)
#     $MasterScript = Join-Path $SolutionDir "DeploymentScripts\Master-Install.ps1"
#     if (Test-Path $MasterScript) {
#         Copy-Item $MasterScript -Destination (Join-Path $Staging "Install-Suite.ps1")
#         Write-Host "📜 Script de instalación inyectado." -ForegroundColor Gray
#     }

#     # Empaquetado Final
#     Write-Progress -Activity "Compilando Suite DAM" -Status "Creando ZIP..." -PercentComplete 95
#     if (!(Test-Path $ReleaseDir)) { New-Item $ReleaseDir -ItemType Directory }
#     $ZipPath = Join-Path $ReleaseDir "DAM_Release_$BuildStamp.zip"
#     Compress-Archive -Path "$Staging\*" -DestinationPath $ZipPath -Force

#     # Limpieza y Retención (Mantener últimos 5)
#     Remove-Item $Staging -Recurse -Force
#     $Old = Get-ChildItem $ReleaseDir -Filter "*.zip" | Sort-Object LastWriteTime -Descending
#     if ($Old.Count -gt 5) { $Old | Select-Object -Skip 5 | Remove-Item -Force }

#     Write-Host "`n✅ RELEASE LISTO: $ZipPath" -ForegroundColor Green
# }
# catch {
#     Write-Host "`n❌ ERROR CRÍTICO: $($_.Exception.Message)" -ForegroundColor Red
#     exit 1
# }
# finally { Write-Progress -Activity "Compilando Suite DAM" -Completed }


# Create-Release.ps1
# Script de empaquetado profesional con transformación dinámica y UX mejorada.

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

# --- Funciones de Transformación Inteligente ---
function Transform-AppSettings {
    param ([string]$Path, [string]$ProjectName)
    
    if (Test-Path $Path) {
        try {
            $config = Get-Content $Path -Raw | ConvertFrom-Json
            $changed = $false

            # 1. Transformación de ConnectionStrings (Detecta múltiples nombres)
            if ($config.ConnectionStrings) {
                # Caso API: DefaultConnection
                if ($config.ConnectionStrings.DefaultConnection) {
                    $config.ConnectionStrings.DefaultConnection = "Data Source=$DbPath"
                    $changed = $true
                }
                # Caso Worker: SQLiteConnection
                if ($config.ConnectionStrings.SQLiteConnection) {
                    $config.ConnectionStrings.SQLiteConnection = "Data Source=$DbPath"
                    $changed = $true
                }
            }

            # 2. Transformación de Serilog (Rutas de Log)
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
                Write-Host "   [CONFIG] AppSettings de '$ProjectName' actualizado correctamente." -ForegroundColor DarkGray
            }
        }
        catch {
            Write-Warning "No se pudo transformar ${Path}: $($_.Exception.Message)"
        }
    }
}

# --- Proceso Principal ---
try {
    Write-Host "🚀 Iniciando Generación de Release: $BuildStamp" -ForegroundColor Cyan
    $Staging = Join-Path $env:TEMP "DAM_Build_$BuildStamp"
    New-Item $Staging -ItemType Directory -Force | Out-Null

    $i = 0
    foreach ($Proj in $Projects) {
        $i++
        $percent = ($i / $Projects.Count) * 80
        # MEJORA DE UX: Barra de progreso con estado granular
        Write-Progress -Activity "Compilando Suite DAM" -Status "Procesando NuGet para: $($Proj.Name)" -PercentComplete $percent

        $TargetFolder = Join-Path $Staging $Proj.Name
        Write-Host "📦 Publicando $($Proj.Name)..." -ForegroundColor Gray
        
        # Publicación .NET
        # MEJORA DE UX: Barra de progreso para la fase de compilación
        Write-Progress -Activity "Compilando Suite DAM" -Status "Publicando: $($Proj.Name)" -PercentComplete $percent
        & dotnet publish $Proj.Path -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $TargetFolder --nologo
        
        if ($LASTEXITCODE -ne 0) { throw "Error publicando $($Proj.Name)" }
        
        # MEJORA DE UX: Barra de progreso para la fase de configuración
        Write-Progress -Activity "Compilando Suite DAM" -Status "Configurando '$($Proj.Name)' para producción" -PercentComplete $percent
        # Inyectar configuración de producción detectando estructura
        Transform-AppSettings -Path (Join-Path $TargetFolder "appsettings.json") -ProjectName $Proj.Name
    }

    # Copiar Script de Instalación (Master-Install -> Install-Suite)
    $MasterScript = Join-Path $SolutionDir "DeploymentScripts\Master-Install.ps1"
    if (Test-Path $MasterScript) {
        Copy-Item $MasterScript -Destination (Join-Path $Staging "Install-Suite.ps1")
        Write-Host "📜 Script de instalación inyectado." -ForegroundColor Gray
    }

    # Empaquetado Final
    # MEJORA DE UX: Barra de progreso para la fase de ZIP
    Write-Progress -Activity "Compilando Suite DAM" -Status "Creando paquete ZIP final..." -PercentComplete 95
    if (!(Test-Path $ReleaseDir)) { New-Item $ReleaseDir -ItemType Directory }
    $ZipPath = Join-Path $ReleaseDir "DAM_Release_$BuildStamp.zip"
    Compress-Archive -Path "$Staging\*" -DestinationPath $ZipPath -Force

    # Limpieza y Retención (Mantener últimos 5)
    Remove-Item $Staging -Recurse -Force
    $Old = Get-ChildItem $ReleaseDir -Filter "*.zip" | Sort-Object LastWriteTime -Descending
    if ($Old.Count -gt 5) { $Old | Select-Object -Skip 5 | Remove-Item -Force }

    Write-Host "`n✅ RELEASE LISTO: $ZipPath" -ForegroundColor Green
}
catch {
    Write-Host "`n❌ ERROR CRÍTICO: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally { Write-Progress -Activity "Compilando Suite DAM" -Completed }