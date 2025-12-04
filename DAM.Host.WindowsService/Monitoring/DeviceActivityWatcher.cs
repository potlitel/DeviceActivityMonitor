using DAM.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Host.WindowsService.Monitoring
{
    // Objeto Watcher por Dispositivo (Unidad)
    public class DeviceActivityWatcher : IDisposable
    {
        private readonly string _driveLetter; // Ej: "E:"
        private readonly FileSystemWatcher _watcher;
        private readonly DeviceActivity _activity;
        private long _initialSize;
        private long _initialAvailable;

        // Evento para notificar al servicio principal cuando el dispositivo se desconecta
        public event Action<DeviceActivity>? ActivityCompleted;

        public string DriveLetter => _driveLetter;
        public DeviceActivity CurrentActivity => _activity;

        public DeviceActivityWatcher(string driveLetter, ILogger<DeviceActivityWatcher> logger)
        {
            _driveLetter = driveLetter;
            _activity = new DeviceActivity
            {
                InsertedAt = DateTime.Now,
                // Llenar más info básica aquí usando WMI si es necesario.
                // Por simplicidad ahora, solo la letra de unidad
                SerialNumber = GetSerialNumber(driveLetter.TrimEnd('\\')),
                Model = GetModel(driveLetter.TrimEnd('\\'))
            };

            // Recolección inicial de capacidad
            var driveInfo = new DriveInfo(driveLetter);
            if (driveInfo.IsReady)
            {
                _initialSize = driveInfo.TotalSize;
                _initialAvailable = driveInfo.AvailableFreeSpace;
                _activity.TotalCapacityMB = driveInfo.TotalSize / (1024 * 1024);
                _activity.InitialAvailableMB = driveInfo.AvailableFreeSpace / (1024 * 1024);
                _activity.FinalAvailableMB = _activity.InitialAvailableMB; // Inicialmente son iguales
            }

            // Configuración del FileSystemWatcher (para COPIA/BORRADO)
            _watcher = new FileSystemWatcher(driveLetter)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            // Eventos para la recolección de métricas
            _watcher.Created += OnFileCreated;
            _watcher.Deleted += OnFileDeleted;
            // Otros eventos como Renamed o Changed pueden ser monitoreados para una lógica más fina.
        }

        // --- Métodos de Recolección de Información ---

        // Simulación: Obtener Número de Serie (requiere WMI más detallado o PInvoke)
        private string GetSerialNumber(string driveRoot) => $"SN-{driveRoot.Replace(":", "")}-{Guid.NewGuid().ToString().Substring(0, 4)}";
        private string GetModel(string driveRoot) => $"Generic USB Drive {driveRoot.Replace(":", "")}";


        // Manejo de Creación de Archivos (COPIA)
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                var fileInfo = new FileInfo(e.FullPath);
                if (fileInfo.Exists)
                {
                    long fileSizeMB = fileInfo.Length / (1024 * 1024);
                    _activity.MegabytesCopied += fileSizeMB;
                    _activity.FilesCopied.Add(e.FullPath);

                    // Actualizar capacidad disponible
                    var driveInfo = new DriveInfo(_driveLetter);
                    if (driveInfo.IsReady)
                    {
                        _activity.FinalAvailableMB = driveInfo.AvailableFreeSpace / (1024 * 1024);
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo robusto de I/O (Fallos en el Watcher)
                // Se debe loggear y potencialmente reiniciar el Watcher si el fallo es crítico.
            }
        }

        // Manejo de Eliminación de Archivos
        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            // En el evento 'Deleted', no podemos obtener el tamaño del archivo, 
            // por lo que nos basamos en el cambio de espacio libre.
            try
            {
                // Suponemos una eliminación.
                _activity.FilesDeleted.Add(e.FullPath);

                // Actualizar capacidad disponible para calcular el espacio borrado/diferencia
                var driveInfo = new DriveInfo(_driveLetter);
                if (driveInfo.IsReady)
                {
                    long newAvailableMB = driveInfo.AvailableFreeSpace / (1024 * 1024);
                    long spaceFreedMB = newAvailableMB - _activity.FinalAvailableMB;

                    if (spaceFreedMB > 0)
                    {
                        _activity.MegabytesDeleted += spaceFreedMB;
                    }
                    _activity.FinalAvailableMB = newAvailableMB;
                }
            }
            catch (Exception ex)
            {
                // Manejo robusto de I/O
            }
        }

        // Método para ser llamado al desconectarse el dispositivo
        public void FinalizeActivity(long finalAvailableMB)
        {
            _activity.ExtractedAt = DateTime.Now;
            _activity.FinalAvailableMB = finalAvailableMB;

            // Reportar la actividad completada al servicio principal
            ActivityCompleted?.Invoke(_activity);
        }

        public void Dispose()
        {
            _watcher.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
