@echo off
REM --- CONFIGURACIÓN DE RUTAS ---
:: Define el nombre del script de PowerShell
set "SCRIPT_NAME=Deploy-Service.ps1"

:: RUTA A DONDE ESTÁ EL PROYECTO .NET CORE
set "PROJECT_PATH=C:\Users\potli\OneDrive\Documentos\Alexis-Cuba\DeviceActivityMonitor\DAM.Host.WindowsService"

:: RUTA DONDE SE INSTALARÁ EL SERVICIO (Usualmente Program Files)
set "DEPLOY_PATH=C:\Program Files\DeviceActivityMonitor"

REM ------------------------------

echo.
echo =======================================================
echo     Despliegue de Device Activity Monitor
echo =======================================================
echo.
echo El proceso de instalación iniciará en una nueva ventana.
echo NOTA: Debe aceptar el dialogo de Administrador (UAC) para continuar.
echo.
pause

:: Comando para ejecutar el script de PowerShell con privilegios de Administrador (RunAs)
:: %~dp0 se expande a la ruta del directorio actual, asegurando que encuentra el PS1.
powershell -Command "Start-Process -FilePath 'powershell.exe' -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File \"%~dp0%SCRIPT_NAME%\" -ProjectPath \"%PROJECT_PATH%\" -DeployPath \"%DEPLOY_PATH%\"' -Verb RunAs"

:: Si el proceso powershell falló (por ejemplo, el usuario canceló el UAC)
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ ERROR: La instalación fue cancelada o no se pudo elevar.
    echo Presione cualquier tecla para salir...
    pause > nul
) else (
    echo.
    echo ✅ Despliegue solicitado. Verifique la nueva ventana de PowerShell para el resultado.
    echo Presione cualquier tecla para salir...
    pause > nul
)