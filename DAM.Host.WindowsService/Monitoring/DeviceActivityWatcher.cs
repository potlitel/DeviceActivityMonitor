using DAM.Core.Entities;
using System.Management;

namespace DAM.Host.WindowsService.Monitoring
{
    /// <summary>
    /// Objeto autónomo responsable de monitorear todas las operaciones de E/S (lectura/escritura) en un dispositivo conectado.
    /// También recopila metadatos iniciales del dispositivo (SN, Modelo, etc).
    /// </summary>
    public class DeviceActivityWatcher : IDisposable
    {
        private readonly string _driveLetter; // Ej: "E:"
        private readonly FileSystemWatcher _watcher;
        private readonly DeviceActivity _activity;
        private readonly ILogger<DeviceActivityWatcher> _logger;

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
            try
            {
                _watcher = new FileSystemWatcher(driveLetter)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.DirectoryName,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };

                // Suscripción a eventos de E/S
                _watcher.Created += OnFileCreated;
                _watcher.Deleted += OnFileDeleted;
                // Manejar errores internos del FileSystemWatcher (robustez)
                _watcher.Error += OnWatcherError;
            }
            catch (ArgumentException ex)
            {
                // Esto ocurre si la letra de unidad no es válida o ya no existe.
                _logger.LogError(ex, "Error al crear FileSystemWatcher para la unidad {DriveLetter}.", driveLetter);
                // Si el watcher no se puede inicializar, el monitoreo de E/S falla, pero el registro básico continúa.
                _watcher = new FileSystemWatcher(); // Inicializar con dummy para evitar NullReferenceException
            }
            // Otros eventos como Renamed o Changed pueden ser monitoreados para una lógica más fina.
        }

        // --- Métodos de Recolección de Información ---

        /// <summary>
        /// Obtiene el Número de Serie de un dispositivo físico correlacionado con una letra de unidad lógica
        /// usando WMI (Windows Management Instrumentation).
        /// </summary>
        /// <param name="driveRoot">La raíz del disco (ej: "E:").</param>
        /// <returns>El número de serie o "UNKNOWN_WMI" si falla.</returns>
        private string GetSerialNumber(string driveRoot)
        {
            // La letra de unidad debe terminar en dos puntos, ej: "E:"
            string driveLetter = driveRoot.TrimEnd('\\');

            try
            {
                // 1. Encontrar la partición asociada al disco lógico
                string queryLogicalDisk = $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}'}} WHERE AssocClass = Win32_LogicalDiskToPartition";

                using (var searcherLogicalDisk = new ManagementObjectSearcher(queryLogicalDisk))
                {
                    foreach (ManagementObject partition in searcherLogicalDisk.Get())
                    {
                        // 2. Encontrar el disco físico asociado a la partición
                        string queryPartition = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_PartitionToDisk";

                        using (var searcherPartition = new ManagementObjectSearcher(queryPartition))
                        {
                            foreach (ManagementObject disk in searcherPartition.Get())
                            {
                                // 3. El número de serie está en el campo 'Signature' o 'PNPDeviceID' en Win32_DiskDrive o 'Serial Number' en Win32_PhysicalMedia
                                // Usaremos el PNPDeviceID o Model para una identificación única.
                                return (disk["PNPDeviceID"]?.ToString() ?? "N/A") + "_" + (disk["Signature"]?.ToString() ?? "N/A");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo WMI al obtener Serial Number para la unidad {DriveLetter}", driveLetter);
            }
            return "UNKNOWN_WMI";
        }

        /// <summary>
        /// Obtiene el Modelo del dispositivo físico asociado a la letra de unidad usando WMI.
        /// </summary>
        /// <param name="driveRoot">La raíz del disco (ej: "E:").</param>
        /// <returns>El modelo o "UNKNOWN_WMI" si falla.</returns>
        private string GetModel(string driveRoot)
        {
            string driveLetter = driveRoot.TrimEnd('\\');
            try
            {
                string queryLogicalDisk = $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}'}} WHERE AssocClass = Win32_LogicalDiskToPartition";

                using (var searcherLogicalDisk = new ManagementObjectSearcher(queryLogicalDisk))
                {
                    foreach (ManagementObject partition in searcherLogicalDisk.Get())
                    {
                        string queryPartition = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_PartitionToDisk";

                        using (var searcherPartition = new ManagementObjectSearcher(queryPartition))
                        {
                            foreach (ManagementObject disk in searcherPartition.Get())
                            {
                                // El modelo se encuentra en el campo 'Model' de Win32_DiskDrive
                                return disk["Model"]?.ToString() ?? "N/A";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo WMI al obtener Modelo para la unidad {DriveLetter}", driveLetter);
            }
            return "UNKNOWN_WMI";
        }


        /// <summary>
        /// Maneja los errores internos del <see cref="FileSystemWatcher"/> (errores de E/S).
        /// Intenta registrar el error y recuperar el watcher.
        /// </summary>
        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            // Registra errores de E/S del FileSystemWatcher y lo reinicia si es necesario (patrón de recuperación).
            _logger.LogError(e.GetException(), "Error I/O fatal en FileSystemWatcher para {DriveLetter}. Intentando recuperar.", _driveLetter);

            // Intento de recuperación: Deshabilitar, esperar y habilitar
            try
            {
                _watcher.EnableRaisingEvents = false;
                Thread.Sleep(5000); // Esperar 5 segundos
                _watcher.EnableRaisingEvents = true;
                _logger.LogWarning("FileSystemWatcher para {DriveLetter} recuperado exitosamente.", _driveLetter);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Fallo al recuperar FileSystemWatcher para {DriveLetter}. Monitoreo de E/S perdido.", _driveLetter);
            }
        }

        /// <summary>
        /// Se activa cuando el <see cref="FileSystemWatcher"/> detecta la creación de un archivo o carpeta.
        /// Actualiza el contador de <see cref="DeviceActivity.MegabytesCopied"/>.
        /// </summary>
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                UpdateMetadataOnFileCreated(e);
            }
            // 1. Manejo específico para archivos que desaparecen antes de poder leerlos
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "El archivo fue creado pero ya no existe en el momento del acceso I/O. Se ignora la actividad: {Path}", e.FullPath);
            }
            // 2. Manejo para errores de acceso o I/O que impiden obtener FileInfo
            catch (IOException ex)
            {
                // Esto puede ocurrir si el archivo está siendo usado por otro proceso (bloqueo)
                _logger.LogWarning(ex, "Fallo de I/O al acceder al archivo creado. Se ignorará esta actividad: {Path}", e.FullPath);
            }
            // 3. Manejo de cualquier otra excepción inesperada
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al procesar evento de creación de archivo: {Path}", e.FullPath);
            }
            // IMPORTANTE: Si ocurre un fallo, el objeto _activity CONSERVA todos los datos acumulados
            // hasta este punto, y el watcher continúa monitoreando eventos futuros.
        }

        /// <summary>
        /// Actualiza los metadatos de la actividad de monitoreo cuando se crea un archivo.
        /// </summary>
        /// <remarks>
        /// Esta función intenta obtener el tamaño del archivo recién creado para sumarlo al total de megabytes copiados.
        /// Además, actualiza la capacidad libre final de la unidad para el seguimiento posterior de eliminaciones o creaciones.
        /// Se incluye manejo de excepciones para fallos de I/O al acceder a la información del archivo o de la unidad.
        /// </remarks>
        /// <param name="e">Datos del evento del sistema de archivos, incluyendo la ruta completa del archivo creado.</param>
        private void UpdateMetadataOnFileCreated(FileSystemEventArgs e)
        {
            var fileInfo = new FileInfo(e.FullPath);
            if (fileInfo.Exists)
            {
                long fileSizeMB = fileInfo.Length / (1024 * 1024);
                _activity.MegabytesCopied += fileSizeMB;
                _activity.FilesCopied.Add(e.FullPath);

                // Actualizar capacidad disponible
                try
                {
                    var driveInfo = new DriveInfo(_driveLetter);
                    if (driveInfo.IsReady)
                    {
                        _activity.FinalAvailableMB = driveInfo.AvailableFreeSpace / (1024 * 1024);
                    }
                }
                catch (IOException ex)
                {
                    _logger.LogWarning(ex, "Fallo de E/S al leer la capacidad disponible del Drive {DriveLetter} después de la creación del archivo. Se omite la actualización de FinalAvailableMB.", _driveLetter);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado al intentar acceder a DriveInfo después de la creación. Drive: {DriveLetter}", _driveLetter);
                }
            }
        }

        /// <summary>
        /// Se activa cuando el <see cref="FileSystemWatcher"/> detecta la eliminación de un archivo o carpeta.
        /// Actualiza el contador de <see cref="DeviceActivity.MegabytesDeleted"/>.
        /// </summary>
        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                UpdateMetadataOnFileDeleted(e);
            }
            // Capturamos cualquier excepción de I/O que pueda ocurrir durante la lectura de DriveInfo
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Fallo de I/O al procesar evento de eliminación. Se ignorará el cálculo de espacio: {Path}", e.FullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al procesar evento de eliminación de archivo: {Path}", e.FullPath);
            }
        }

        /// <summary>
        /// Actualiza los metadatos de la actividad de monitoreo cuando se elimina un archivo.
        /// </summary>
        /// <remarks>
        /// Debido a las limitaciones del evento 'Deleted' de FileSystemWatcher (que no proporciona el tamaño del archivo),
        /// esta función se basa en el **cambio en el espacio libre disponible** de la unidad para estimar la cantidad de espacio liberado.
        /// Si el cálculo del espacio libre falla (por ejemplo, si la unidad no está lista o por un error de E/S), solo se registra el archivo eliminado.
        /// </remarks>
        /// <param name="e">Datos del evento del sistema de archivos, incluyendo la ruta completa del archivo eliminado.</param>
        private void UpdateMetadataOnFileDeleted(FileSystemEventArgs e)
        {
            // En el evento 'Deleted', no podemos obtener el tamaño del archivo, 
            // por lo que nos basamos en el cambio de espacio libre.
            _activity.FilesDeleted.Add(e.FullPath);

            // Actualizar capacidad disponible para calcular el espacio borrado/diferencia
            try
            {
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
                else
                {
                    // Si el disco no está listo, solo registramos el archivo borrado.
                    _logger.LogWarning("Drive {DriveLetter} no estaba listo durante el evento de eliminación para calcular el espacio libre.", _driveLetter);
                }
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Fallo de I/O al acceder a DriveInfo durante la eliminación. Se ignora el cálculo de espacio: {Path}", e.FullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al intentar acceder a DriveInfo durante la eliminación. Drive: {DriveLetter}", _driveLetter);
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
