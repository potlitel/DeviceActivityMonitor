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

        private readonly System.Timers.Timer _debounceTimer;
        private const int DebounceIntervalMs = 1500; // 1.5 segundos de "calma" para estabilizar el disco

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

            // Configuración del Timer de Debounce
            _debounceTimer = new System.Timers.Timer(DebounceIntervalMs)
            {
                AutoReset = false // Solo se dispara una vez tras el último evento
            };
            _debounceTimer.Elapsed += (s, e) => CalculateDiskSpaceDifferential();
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
            _activity.FilesCopied.Add(e.FullPath);
            RestartDebounce();
        }

        /// <summary>
        /// Se activa cuando el <see cref="FileSystemWatcher"/> detecta la eliminación de un archivo o carpeta.
        /// Actualiza el contador de <see cref="DeviceActivity.MegabytesDeleted"/>.
        /// </summary>
        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            _activity.FilesDeleted.Add(e.FullPath);
            RestartDebounce();
        }

        /// <summary>
        /// Reinicia el temporizador de estabilización cada vez que ocurre un evento de E/S.
        /// </summary>
        private void RestartDebounce()
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        /// <summary>
        /// Lógica centralizada para calcular cuánto espacio cambió en la unidad tras un periodo de actividad.
        /// </summary>
        private void CalculateDiskSpaceDifferential()
        {
            try
            {
                var driveInfo = new DriveInfo(_driveLetter);
                if (!driveInfo.IsReady)
                {
                    _logger.LogWarning(Messages.Watcher.DriveNotReady, _driveLetter);
                    return;
                }

                long currentAvailableMB = driveInfo.AvailableFreeSpace / DataConstants.BytesToMbFactor;

                // Diferencia absoluta entre lo que teníamos y lo que hay ahora
                long difference = currentAvailableMB - _activity.FinalAvailableMB;

                if (difference < 0) // El espacio DISMINUYÓ (Copia)
                {
                    long mbCopied = Math.Abs(difference);
                    _activity.MegabytesCopied += mbCopied;
                    _logger.LogDebug(Messages.Watcher.DebounceOcupied, mbCopied, _driveLetter);
                }
                else if (difference > 0) // El espacio AUMENTÓ (Eliminación)
                {
                    long mbFreed = difference;
                    _activity.MegabytesDeleted += mbFreed;

                    _logger.LogDebug(Messages.Watcher.DebounceFreed, mbFreed, _driveLetter);
                }

                // Sincronizamos el estado final
                _activity.FinalAvailableMB = currentAvailableMB;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, Messages.Watcher.DriveException, _driveLetter);
            }
        }

        /// <summary>
        /// Finaliza la actividad de monitoreo, registra la capacidad final y reporta los datos recolectados.
        /// </summary>
        public void FinalizeActivity()
        {
            // 1. Detenemos el timer para evitar que se ejecute en segundo plano después de finalizar
            _debounceTimer.Stop();

            // 2. FORZAMOS el cálculo diferencial final AHORA MISMO.
            // Esto capturará los últimos cambios de espacio antes de cerrar la actividad.
            CalculateDiskSpaceDifferential();

            _activity.ExtractedAt = DateTime.Now;
            _activity.SetTimeInsertedDuration();

            // Reportar la actividad completada
            ActivityCompleted?.Invoke(_activity);
        }

        /// <summary>
        /// Libera los recursos del <see cref="FileSystemWatcher"/>.
        /// </summary>
        public void Dispose()
        {
            _debounceTimer.Dispose();
            _watcher.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
