using DAM.Core.Constants;
using DAM.Core.Entities;
using DAM.Host.WindowsService.Monitoring.Interfaces;
using DAM.Infrastructure.Utils;

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
            long mbConversionFactor = DataConstants.BytesToMbFactor;

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
                    SerialNumber = WMI_Utils.GetPersistentSerialNumber(driveLetter.TrimEnd('\\'), _logger),
                    Model = WMI_Utils.GetModel(driveLetter.TrimEnd('\\'), _logger),
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
                    SerialNumber = DataConstants.UnknownError,
                    Model = DataConstants.UnknownError,
                    TotalCapacityMB = 0,
                    InitialAvailableMB = 0,
                    FinalAvailableMB = 0
                };

                _logger.LogWarning(Messages.Watcher.DriveNotReady, driveLetter);
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
                _logger.LogError(ex, Messages.Watcher.WatcherCreationError, driveLetter);

                // Inicializar con dummy para evitar NullReferenceException en el resto de la clase.
                // FileSystemWatcher sin ruta de monitoreo.
                _watcher = new FileSystemWatcher();
            }
        }

        /// <summary>
        /// Maneja los errores internos del <see cref="FileSystemWatcher"/> (errores de E/S).
        /// Intenta registrar el error y recuperar el watcher.
        /// </summary>
        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            // Registra errores de E/S del FileSystemWatcher y lo reinicia si es necesario (patrón de recuperación).
            _logger.LogError(e.GetException(), Messages.Watcher.WatcherIoFatal, _driveLetter);

            // Intento de recuperación: Deshabilitar, esperar y habilitar
            try
            {
                _watcher.EnableRaisingEvents = false;
                Thread.Sleep(5000); // Esperar 5 segundos
                _watcher.EnableRaisingEvents = true;
                _logger.LogWarning(Messages.Watcher.WatcherRecovered, _driveLetter);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, Messages.Watcher.WatcherRecoverFailed, _driveLetter);
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
                _logger.LogWarning(ex, Messages.Watcher.FileNotFound, e.FullPath);
            }
            // 2. Manejo para errores de acceso o I/O que impiden obtener FileInfo
            catch (IOException ex)
            {
                // Esto puede ocurrir si el archivo está siendo usado por otro proceso (bloqueo)
                _logger.LogWarning(ex, Messages.Watcher.FileIoError, e.FullPath);
            }
            // 3. Manejo de cualquier otra excepción inesperada
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Watcher.UnexpectedError, e.FullPath);
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
            //var fileInfo = new FileInfo(e.FullPath);
            //if (fileInfo.Exists)
            //{
            //    long fileSizeMB = fileInfo.Length / DataConstants.BytesToMbFactor;
            //    _activity.MegabytesCopied += fileSizeMB;
            //    _activity.FilesCopied.Add(e.FullPath);

            //    // Actualizar capacidad disponible
            //    RefreshFinalAvailableSpace();
            //}
            // 1. Registramos el nombre del archivo inmediatamente
            _activity.FilesCopied.Add(e.FullPath);

            // 2. En lugar de leer fileInfo.Length (que suele ser 0 al empezar la copia),
            // calculamos cuánto espacio ha DISMINUIDO en la unidad desde la última operación.
            try
            {
                var driveInfo = new DriveInfo(_driveLetter);
                if (driveInfo.IsReady)
                {
                    long currentAvailableMB = driveInfo.AvailableFreeSpace / DataConstants.BytesToMbFactor;

                    // Si el espacio actual es MENOR que el último registrado, es que se ha ocupado espacio.
                    long spaceOccupiedMB = _activity.FinalAvailableMB - currentAvailableMB;

                    if (spaceOccupiedMB > 0)
                    {
                        _activity.MegabytesCopied += spaceOccupiedMB;
                    }

                    // Actualizamos el marcador de posición para la siguiente operación
                    _activity.FinalAvailableMB = currentAvailableMB;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, Messages.Watcher.DriveException, _driveLetter);
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
                _logger.LogWarning(ex, Messages.Watcher.DeleteIoError, e.FullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Watcher.UnexpectedError, e.FullPath);
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
            //// En el evento 'Deleted', no podemos obtener el tamaño del archivo, 
            //// por lo que nos basamos en el cambio de espacio libre.
            //_activity.FilesDeleted.Add(e.FullPath);

            //// Actualizar capacidad disponible para calcular el espacio borrado/diferencia
            //try
            //{
            //    var driveInfo = new DriveInfo(_driveLetter);

            //    if (driveInfo.IsReady)
            //    {
            //        long newAvailableMB = driveInfo.AvailableFreeSpace / DataConstants.BytesToMbFactor;
            //        long spaceFreedMB = newAvailableMB - _activity.FinalAvailableMB;

            //        if (spaceFreedMB > 0)
            //        {
            //            _activity.MegabytesDeleted += spaceFreedMB;
            //        }
            //        _activity.FinalAvailableMB = newAvailableMB;
            //    }
            //    else
            //    {
            //        // Si el disco no está listo, solo registramos el archivo borrado.
            //        _logger.LogWarning(Messages.Watcher.DriveNotReadyDelete, _driveLetter);
            //    }
            //}
            //catch (IOException ex)
            //{
            //    _logger.LogWarning(ex, Messages.Watcher.DriveNotReadyI0Delete, e.FullPath);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, Messages.Watcher.DriveInfoError, _driveLetter);
            //}
            _activity.FilesDeleted.Add(e.FullPath);

            try
            {
                var driveInfo = new DriveInfo(_driveLetter);
                if (driveInfo.IsReady)
                {
                    long currentAvailableMB = driveInfo.AvailableFreeSpace / DataConstants.BytesToMbFactor;

                    // Si el espacio actual es MAYOR que el último registrado, es que se ha liberado espacio.
                    long spaceFreedMB = currentAvailableMB - _activity.FinalAvailableMB;

                    if (spaceFreedMB > 0)
                    {
                        _activity.MegabytesDeleted += spaceFreedMB;
                    }

                    _activity.FinalAvailableMB = currentAvailableMB;
                }
                else
                {
                    _logger.LogWarning(Messages.Watcher.DriveNotReadyDelete, _driveLetter);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Watcher.DriveInfoError, _driveLetter);
            }
        }

        /// <summary>
        /// Actualiza el espacio libre final disponible en la unidad y lo registra en la actividad actual.
        /// </summary>
        /// <remarks>
        /// Este método consulta el estado actual del hardware a través de <see cref="DriveInfo"/>. 
        /// Es fundamental para calcular la diferencia de espacio tras operaciones de creación o eliminación.
        /// Si la unidad no está lista o se produce un error de E/S, el valor de 
        /// <see cref="DeviceActivity.FinalAvailableMB"/> conservará su último valor conocido.
        /// </remarks>
        /// <exception cref="IOException">
        /// Capturada internamente si hay un error de E/S al acceder a la unidad (ej. desconexión repentina).
        /// </exception>
        /// <exception cref="Exception">
        /// Capturada internamente para cualquier error inesperado del sistema de archivos.
        /// </exception>
        private void RefreshFinalAvailableSpace()
        {
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
                _logger.LogWarning(ex, Messages.Watcher.DriveIOException, _driveLetter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Watcher.DriveException, _driveLetter);
            }
        }

        /// <summary>
        /// Finaliza la actividad de monitoreo, registra la capacidad final y reporta los datos recolectados.
        /// </summary>
        public void FinalizeActivity()
        {
            RefreshFinalAvailableSpace();

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
