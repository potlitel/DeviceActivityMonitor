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

# --- 3. Empaquetar para Distribución ---
Write-Host "3. Creando paquete de despliegue ZIP..." -ForegroundColor Green
Remove-Item -Path $ReleaseDir -Recurse -Force -ErrorAction SilentlyContinue
mkdir $ReleaseDir | Out-Null

$ZipPath = Join-Path -Path $ReleaseDir -ChildPath $ReleaseZipName

# Crear el archivo ZIP (se excluye el .pdb para reducir tamaño)
Compress-Archive -Path "$PublishTempDir\*" -DestinationPath $ZipPath -Update

Write-Host ""
Write-Host "✅ ¡Paquete de Despliegue Listo!" -ForegroundColor Cyan
Write-Host "Ruta del paquete: $ZipPath" -ForegroundColor Cyan
Write-Host "Instrucción para el usuario: Descomprima el ZIP y ejecute 'Install-Service.ps1' como Administrador." -ForegroundColor Yellow