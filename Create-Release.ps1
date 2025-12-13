# Create-Release.ps1
# Script para automatizar la publicación y el empaquetado del servicio de Windows.

# --- Configuración ---
$SolutionDir = Split-Path -Parent $PSCommandPath
$ProjectName = "DAM.Host.WindowsService"
$ProjectPath = Join-Path -Path $SolutionDir -ChildPath "$ProjectName\$ProjectName.csproj"
$PublishTempDir = Join-Path -Path $SolutionDir -ChildPath "bin\Release\PublishTemp"
$ReleaseDir = Join-Path -Path $SolutionDir -ChildPath "Releases"
$ReleaseZipName = "$ProjectName-Release-$((Get-Date).ToString('yyyyMMdd-HHmmss')).zip"
$DeploymentScriptSource = Join-Path -Path $SolutionDir -ChildPath "$ProjectName\DeploymentScritps\Deploy-Service.ps1"
$DeploymentScriptTarget = Join-Path -Path $PublishTempDir -ChildPath "Install-Service.ps1" # Renombrado para simplicidad

# --- Configuración de Retención ---
$MaxReleasesToKeep = 5 # Mantener los 5 últimos paquetes de despliegue ZIP.

# --- 1. Publicar el Proyecto .NET ---
Write-Host "1. Publicando proyecto '$ProjectName'..." -ForegroundColor Green
Remove-Item -Path $PublishTempDir -Recurse -Force -ErrorAction SilentlyContinue
mkdir $PublishTempDir | Out-Null

& dotnet publish $ProjectPath -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $PublishTempDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "La publicación de .NET falló."
    exit 1
}

# --- 2. Copiar el Script de Despliegue Relativo ---
Write-Host "2. Copiando script de despliegue ajustado..." -ForegroundColor Green
Copy-Item -Path $DeploymentScriptSource -Destination $DeploymentScriptTarget -Force
# NOTA: Asegúrate que Deploy-Service.ps1 haya sido ajustado (Paso 1)

# --- 3. Generar y Copiar Release Notes ---
Write-Host "3. Generando archivo de notas de release..." -ForegroundColor Green

$ReleaseNotesContent = @"
Release Date: $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))
Project: $ProjectName
Version: 1.0.0.0 (Actualiza esta línea manualmente o con CI/CD)
Commit Hash: N/A (Se puede añadir integración con Git)

Instrucción de Instalación:
1. Descomprima este ZIP.
2. Ejecute 'Install-Service.ps1' como Administrador.
"@

$ReleaseNotesPath = Join-Path -Path $PublishTempDir -ChildPath "RELEASE_NOTES.txt"
$ReleaseNotesContent | Out-File $ReleaseNotesPath -Encoding UTF8

# --- 4. Empaquetar para Distribución (Incluye la Nueva Política) ---
Write-Host "4. Creando paquete de despliegue ZIP..." -ForegroundColor Green
# No borramos la carpeta $ReleaseDir, solo nos aseguramos de que exista.
if (-not (Test-Path $ReleaseDir -PathType Container)) {
    New-Item -Path $ReleaseDir -ItemType Directory -Force | Out-Null
}

$ZipPath = Join-Path -Path $ReleaseDir -ChildPath $ReleaseZipName

# Crear el archivo ZIP (se excluye el .pdb para reducir tamaño)
Compress-Archive -Path "$PublishTempDir\*" -DestinationPath $ZipPath -Update

# --- 5. Aplicar Política de Retención (Nuevo Paso) ---
Write-Host "5. Aplicando política de retención (manteniendo los últimos $MaxReleasesToKeep releases)..." -ForegroundColor Yellow

$OldReleases = Get-ChildItem -Path $ReleaseDir -Filter "*.zip" | Sort-Object LastWriteTime -Descending

if ($OldReleases.Count -gt $MaxReleasesToKeep) {
    # Selecciona los que están más allá del límite
    $ReleasesToDelete = $OldReleases | Select-Object -Skip $MaxReleasesToKeep
    
    foreach ($Release in $ReleasesToDelete) {
        Write-Host "   -> Eliminando release antiguo: $($Release.Name)" -ForegroundColor DarkGray
        Remove-Item -Path $Release.FullName -Force
    }
} else {
    Write-Host "   -> Número de releases menor o igual al límite. No se requiere limpieza." -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "✅ ¡Paquete de Despliegue Listo!" -ForegroundColor Cyan
Write-Host "Ruta del paquete: $ZipPath" -ForegroundColor Cyan
Write-Host "Instrucción para el usuario: Descomprima el ZIP y ejecute 'Install-Service.ps1' como Administrador." -ForegroundColor Yellow