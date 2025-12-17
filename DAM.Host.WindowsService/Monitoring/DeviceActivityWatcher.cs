using DAM.Core.Constants;
using DAM.Core.Entities;
using DAM.Host.WindowsService.Monitoring.Interfaces;
using System.Management;

namespace DAM.Host.WindowsService.Monitoring
{
    /// <summary>
    /// Objeto autónomo responsable de monitorear todas las operaciones de E/S (lectura/escritura) en un dispositivo conectado.
    /// También recopila metadatos iniciales del dispositivo (SN, Modelo, etc).
    /// </summary>
    public class DeviceActivityWatcher : IDeviceActivityWatcher
    {
        private readonly string _driveLetter; // Ej: "E:"
        private FileSystemWatcher _watcher = null!;
        private DeviceActivity _activity = null!;
        private readonly ILogger<DeviceActivityWatcher> _logger;

        // Campos privados para la capacidad.
        private long _initialTotalCapacity;
        private long _initialAvailableSpace;

        /// <summary>
        /// Evento disparado cuando el dispositivo se desconecta y se completa la recolección de datos.
        /// </summary>
        public event Action<DeviceActivity>? ActivityCompleted;

        public string DriveLetter => _driveLetter;

        /// <summary>
        /// Obtiene la actividad recopilada actualmente. Usada para pruebas y diagnóstico.
        /// </summary>
        public DeviceActivity CurrentActivity => _activity;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="DeviceActivityWatcher"/>.
        /// </summary>
        /// <param name="driveLetter">La letra de unidad asignada al dispositivo (ej: "E:").</param>
        /// <param name="logger">El servicio de logging.</param>
        /// <remarks>
        /// Este constructor separa la lógica de inicialización en dos métodos privados:
        /// <list type="bullet">
        /// <item>Inicialización de la actividad y metadatos del disco.</item>
        /// <item>Configuración y suscripción del <see cref="FileSystemWatcher"/>.</item>
        /// </list>
        /// </remarks>
        public DeviceActivityWatcher(string driveLetter, ILogger<DeviceActivityWatcher> logger)
        {
            _driveLetter = driveLetter;
            _logger = logger;

            InitializeDriveMetadata(driveLetter);

            SetupFileSystemWatcher(driveLetter);
        }

        /// <summary>
        /// Recolecta la información inicial del disco (<see cref="DriveInfo"/>) e inicializa el objeto <see cref="DeviceActivity"/>.
        /// </summary>
        /// <param name="driveLetter">La letra de unidad para la cual se recopilan los metadatos.</param>
        /// <returns>void</returns>
        /// <remarks>
        /// Si el disco no está listo, la actividad se inicializa con valores predeterminados ("UNKNOWN_ERR" o 0)
        /// y se registra una advertencia.
        /// </remarks>
        private void InitializeDriveMetadata(string driveLetter)
        {
            var driveInfo = new DriveInfo(driveLetter);
            long mbConversionFactor = 1024 * 1024;

            if (driveInfo.IsReady)
            {
                // Almacenar valores brutos (Bytes)
                _initialTotalCapacity = driveInfo.TotalSize;
                _initialAvailableSpace = driveInfo.AvailableFreeSpace;

                // Inicialización de la entidad
                _activity = new DeviceActivity
                {
                    InsertedAt = DateTime.Now,
                    // Conversión a MB para el registro
                    TotalCapacityMB = _initialTotalCapacity / mbConversionFactor,
                    InitialAvailableMB = _initialAvailableSpace / mbConversionFactor,
                    FinalAvailableMB = _initialAvailableSpace / mbConversionFactor, // Inicialmente son iguales

                    // Metadatos que requerirían WMI real en producción
                    SerialNumber = GetSerialNumberImproved(driveLetter.TrimEnd('\\')),
                    Model = GetModel(driveLetter.TrimEnd('\\')),
                };
            }
            else
            {
                // Manejo de caso donde el disco no está listo.
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

                _logger.LogWarning("Drive {Drive} not ready upon insertion. Activity logging may be incomplete.", driveLetter);
            }
        }

        /// <summary>
        /// Configura el <see cref="FileSystemWatcher"/> para la unidad especificada y suscribe los manejadores de eventos.
        /// </summary>
        /// <param name="driveLetter">La letra de unidad a monitorear.</param>
        /// <returns>void</returns>
        /// <exception cref="ArgumentException">Se lanza si la letra de unidad no es válida o no existe al intentar crear el watcher.</exception>
        private void SetupFileSystemWatcher(string driveLetter)
        {
            try
            {
                _watcher = new FileSystemWatcher(driveLetter)
                {
                    // Filtros para monitorear creación, eliminación, cambios de nombre y tamaño.
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.DirectoryName,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true // Iniciar el monitoreo inmediatamente
                };

                // Suscripción a eventos de E/S
                _watcher.Created += OnFileCreated;
                _watcher.Deleted += OnFileDeleted;
                // Manejar errores internos del FileSystemWatcher (robustez)
                _watcher.Error += OnWatcherError;
            }
            catch (ArgumentException ex)
            {
                // Loggear el error si el watcher no puede ser inicializado.
                _logger.LogError(ex, "Error al crear FileSystemWatcher para la unidad {DriveLetter}. El monitoreo de E/S no estará activo.", driveLetter);

                // Inicializar con dummy para evitar NullReferenceException en el resto de la clase.
                // FileSystemWatcher sin ruta de monitoreo.
                _watcher = new FileSystemWatcher();
            }
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

        private string GetSerialNumberImproved(string driveRoot)
        {
            // Asegura que la letra de unidad tenga el formato "E:"
            string driveLetter = driveRoot.TrimEnd('\\');

            // 1. Intentar obtener el Serial Number (SN) directamente desde Win32_PhysicalMedia
            try
            {
                // La consulta busca el Physical Media asociado al Logical Disk.
                string queryPhysicalMedia = string.Format(WmiQueries.LogicalDiskToPartition, driveLetter);

                using (var searcher = new ManagementObjectSearcher(queryPhysicalMedia))
                {
                    foreach (ManagementObject partition in searcher.Get())
                    {
                        // Ahora asociamos la partición con el disco físico real
                        string queryDiskDrive = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_PartitionToDisk";
                        using (var searcherDisk = new ManagementObjectSearcher(queryDiskDrive))
                        {
                            foreach (ManagementObject disk in searcherDisk.Get())
                            {
                                // Encontrar la Win32_PhysicalMedia asociada a este disco
                                string queryPhysical = $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{disk["DeviceID"]}'}} WHERE AssocClass = Win32_DiskDriveToDiskMedia";
                                using (var searcherPhysical = new ManagementObjectSearcher(queryPhysical))
                                {
                                    foreach (ManagementObject physicalMedia in searcherPhysical.Get())
                                    {
                                        // El campo SerialNumber de Win32_PhysicalMedia es el que queremos.
                                        string serialNumber = physicalMedia["SerialNumber"]?.ToString().Trim();

                                        // Si encontramos un SN válido, lo retornamos inmediatamente.
                                        if (!string.IsNullOrEmpty(serialNumber) && serialNumber != "0")
                                        {
                                            return serialNumber;
                                        }
                                        // Si no se encuentra SerialNumber en Win32_PhysicalMedia, 
                                        // podemos usar la información de Win32_DiskDrive (tu enfoque anterior)
                                        else
                                        {
                                            string pnpID = disk["PNPDeviceID"]?.ToString() ?? "N/A";
                                            string signature = disk["Signature"]?.ToString() ?? "N/A";
                                            // Retorna un identificador compuesto robusto si el SN directo falla.
                                            //return $"PNP_{pnpID}_{signature}";
                                            return $"{DataConstants.PnpPrefix}{pnpID}_{signature}";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // En caso de que falle toda la lógica WMI, registra el error y retorna un identificador único por falla.
                _logger.LogError(ex, "Fallo WMI al obtener Serial Number para la unidad {DriveLetter}", driveLetter);
            }

            // Si todo falla (incluyendo la excepción try/catch), genera un identificador único de fallback.
            // Esto asegura que NUNCA se repita UNKNOWN_WMI.
            //return $"WMI_FAIL_{Guid.NewGuid().ToString()}";
            return $"{DataConstants.WmiFailPrefix}{Guid.NewGuid()}";
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
        public void FinalizeActivity()
        {
            _activity.ExtractedAt = DateTime.Now;

            _activity.SetTimeInsertedDuration();

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
