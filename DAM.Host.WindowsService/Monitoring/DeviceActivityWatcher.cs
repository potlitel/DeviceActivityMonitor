using DAM.Core.Entities;

namespace DAM.Host.WindowsService.Monitoring
{
    /// <summary>
    /// Objeto autónomo responsable de monitorear todas las operaciones de E/S (lectura/escritura) en un dispositivo conectado.
    /// </summary>
    public class DeviceActivityWatcher : IDisposable
    {
        private readonly string _driveLetter; // Ej: "E:"
        private readonly FileSystemWatcher _watcher;
        private readonly DeviceActivity _activity;

        // Campos privados para la capacidad.
        private readonly long _initialTotalCapacity;
        private readonly long _initialAvailableSpace;

        /// <summary>
        /// Evento disparado cuando el dispositivo se desconecta y se completa la recolección de datos.
        /// </summary>
        public event Action<DeviceActivity>? ActivityCompleted;

        public string DriveLetter => _driveLetter;
        public DeviceActivity CurrentActivity => _activity;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="DeviceActivityWatcher"/>.
        /// </summary>
        /// <param name="driveLetter">La letra de unidad asignada al dispositivo (ej: "E:").</param>
        /// <param name="logger">El servicio de logging.</param>
        public DeviceActivityWatcher(string driveLetter, ILogger<DeviceActivityWatcher> logger)
        {
            _driveLetter = driveLetter;

            // --- 1. Recolección de Información de Capacidad y Metadatos ---
            var driveInfo = new DriveInfo(driveLetter);

            if (driveInfo.IsReady)
            {
                // Almacenar valores brutos (Bytes)
                _initialTotalCapacity = driveInfo.TotalSize;
                _initialAvailableSpace = driveInfo.AvailableFreeSpace;

                // Inicialización de la entidad
                _activity = new DeviceActivity
                {
                    InsertedAt = DateTime.Now,
                    // Conversión a MB para el registro (1024 * 1024)
                    TotalCapacityMB = _initialTotalCapacity / (1024 * 1024),
                    InitialAvailableMB = _initialAvailableSpace / (1024 * 1024),
                    FinalAvailableMB = _initialAvailableSpace / (1024 * 1024), // Inicialmente son iguales

                    // Metadatos que requerirían WMI real en producción
                    SerialNumber = GetSerialNumber(driveLetter.TrimEnd('\\')),
                    Model = GetModel(driveLetter.TrimEnd('\\')),
                };
            }
            else
            {
                // Manejo de caso donde el disco no está listo inmediatamente (ej. fallo de montaje)
                // Se inicializa con valores predeterminados y se loggea la advertencia.
                _initialTotalCapacity = 0;
                _initialAvailableSpace = 0;

                _activity = new DeviceActivity
                {
                    InsertedAt = DateTime.Now,
                    SerialNumber = "UNKNOWN_ERR",
                    Model = "UNKNOWN_ERR",
                    TotalCapacityMB = 0,
                    InitialAvailableMB = 0,
                    FinalAvailableMB = 0
                };

                logger.LogWarning("Drive {Drive} not ready upon insertion. Activity logging may be incomplete.", driveLetter);
            }

            // --- 2. Configuración del FileSystemWatcher ---
            _watcher = new FileSystemWatcher(driveLetter)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            // Suscripción a eventos de E/S
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

        /// <summary>
        /// Finaliza la actividad de monitoreo, registra la capacidad final y reporta los datos recolectados.
        /// </summary>
        /// <param name="finalAvailableMB">La capacidad disponible al momento de la extracción.</param>
        public void FinalizeActivity(long finalAvailableMB)
        {
            _activity.ExtractedAt = DateTime.Now;
            _activity.FinalAvailableMB = finalAvailableMB;

            // Reportar la actividad completada al servicio principal
            ActivityCompleted?.Invoke(_activity);
        }

        /// <summary>
        /// Libera los recursos del <see cref="FileSystemWatcher"/>.
        /// </summary>
        public void Dispose()
        {
            _watcher.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
