using DAM.Core.Constants;
using DAM.Core.DTOs.Heartbeat;
using DAM.Core.Entities;
using DAM.Core.Interfaces;
using DAM.Host.WindowsService.Monitoring;
using DAM.Host.WindowsService.Monitoring.Interfaces;
using System.Collections.Concurrent;

namespace DAM.Host.WindowsService;

/// <summary>
/// Servicio principal de Windows que orquesta la detección de dispositivos y el ciclo de vida de los Watchers.
/// </summary>
/// <remarks>
/// Hereda de <see cref="BackgroundService"/> para ejecutarse como un servicio de larga duración.
/// Utiliza <see cref="IServiceScopeFactory"/> para gestionar la persistencia resiliente de manera segura para Singletón/Scoped.
/// </remarks>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IDeviceMonitor _deviceMonitor;
    private readonly ILoggerFactory _loggerFactory;
    //private readonly IActivityStorageService _storageService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDeviceActivityWatcherFactory _watcherFactory;
    private readonly IDevicePersistenceService _devicePersistenceService;

    /// <summary>
    /// Colección concurrente para mantener un <see cref="DeviceActivityWatcher"/> activo por cada dispositivo conectado.
    /// </summary>
    private readonly ConcurrentDictionary<string, DeviceActivityWatcher> _activeWatchers = new();

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="Worker"/>.
    /// </summary>
    /// <param name="logger">Servicio de logging para el Worker.</param>
    /// <param name="deviceMonitor">Monitor de eventos de hardware (WMI).</param>
    /// <param name="scopeFactory">Factoría para la creación de ámbitos de servicio, permitiendo el uso de servicios Scoped (como DbContext) desde este Singleton.</param>
    public Worker(ILogger<Worker> logger, IDeviceMonitor deviceMonitor, ILoggerFactory loggerFactory,
                  IServiceScopeFactory scopeFactory, IDeviceActivityWatcherFactory watcherFactory, 
                  IDevicePersistenceService devicePersistenceService)
    {
        _logger = logger;
        _deviceMonitor = deviceMonitor;
        _loggerFactory = loggerFactory;
        _scopeFactory = scopeFactory;
        _watcherFactory = watcherFactory;
        _devicePersistenceService = devicePersistenceService;
    }

    /// <summary>
    /// Método principal que contiene la lógica de ejecución del servicio de larga duración.
    /// Aquí se inicializan las dependencias, se registra el evento de inicio del servicio, 
    /// y se inicia el monitoreo WMI para detectar la conexión de dispositivos.
    /// </summary>
    /// <inheritdoc path="param[@name='stoppingToken']" />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(WorkerMessages.Log.ServiceStarting, DateTimeOffset.Now);

        await _devicePersistenceService.RecoverPendingActivitiesAsync();

        // 1. Registrar evento de inicio en la base de datos (Persistencia de Eventos)
        await _devicePersistenceService.PersistServiceEventAsync(new ServiceEvent
        {
            Timestamp = DateTime.Now,
            EventType = WorkerMessages.ServiceEventTypes.ServiceStart,
            Message = WorkerMessages.ServiceEventMessages.ServiceStarted
        });

        // 2. Configurar y arrancar el monitoreo WMI
        // Suscribirse a eventos de conexión/desconexión
        _deviceMonitor.DeviceConnected += HandleDeviceConnected;
        _deviceMonitor.DeviceDisconnected += HandleDeviceDisconnected;
        _deviceMonitor.StartMonitoring();

        _logger.LogInformation(WorkerMessages.Log.MonitoringStarted);

        // 3. BUCLE DE HEARTBEAT (Latido de corazón)
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendHeartbeatAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("No se pudo enviar el latido de corazón: {Message}", ex.Message);
            }

            // Esperar 30 segundos antes del próximo latido
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        // El Worker se mantiene en ejecución hasta que se solicita la detención.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private DateTime _startTime = DateTime.Now;

    private async Task SendHeartbeatAsync()
    {
        try
        {
            using var process = System.Diagnostics.Process.GetCurrentProcess();

            // Calculamos el UpTime
            var upTime = (DateTime.Now - _startTime).TotalSeconds;

            // Captura de métricas de memoria
            long memoryMB = process.WorkingSet64 / (1024 * 1024);

            // Captura de CPU (Aproximación por tiempo de procesador)
            // Nota: En Windows, esto mide el tiempo total de CPU usado por el proceso
            var cpuUsage = Math.Round(process.TotalProcessorTime.TotalMilliseconds / (DateTime.Now - _startTime).TotalMilliseconds * 100, 2);

            var heartbeat = new HeartbeatDto(
                ServiceName: "Device Activity Monitor (DAM)",
                MachineName: Environment.MachineName,
                Status: "Running", //Se debe modificar, este dato no puede ser fijo!!!
                ActiveWatchers: _activeWatchers.Count,
                CpuUsagePercentage: cpuUsage,
                MemoryUsageMB: memoryMB,
                UpTimeSeconds: (long)upTime,
                Version: System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
                Timestamp: DateTime.Now
            );

            // Enviamos a través del servicio resiliente
            await _devicePersistenceService.SendHeartbeatAsync(heartbeat);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error al recolectar métricas de salud: {Message}", ex.Message);
        }
    }

    // --- Manejo de Eventos ---
    /// <summary>
    /// Handler que se activa cuando el <see cref="IDeviceMonitor"/> detecta la conexión de un nuevo dispositivo.
    /// </summary>
    /// <param name="driveLetter">La letra de unidad asignada al dispositivo (ej: "E:").</param>
    private void HandleDeviceConnected(string driveLetter)
    {
        _logger.LogInformation(WorkerMessages.Log.DeviceConnected, driveLetter);

        // Aseguramos que solo haya un watcher por unidad
        if (!_activeWatchers.ContainsKey(driveLetter))
        {
            var watcherLogger = _loggerFactory.CreateLogger<DeviceActivityWatcher>();
            var watcher = _watcherFactory.Create(driveLetter, watcherLogger);

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    // 2. REGISTRAR PRESENCIA (Llama al método que usa scope y persiste)
                    await _devicePersistenceService.PersistPresenceAsync(watcher.CurrentActivity);
                    watcher.ActivityCompleted += HandleActivityCompleted;
                    _activeWatchers.TryAdd(driveLetter, (DeviceActivityWatcher)watcher);
                    _logger.LogInformation(WorkerMessages.Log.ActivityProcessed, watcher.CurrentActivity.SerialNumber);
                }
                catch (Exception ex)
                {
                    // Nota: El servicio de persistencia ya debería haber logeado los errores específicos,
                    // pero es buena práctica logear el fallo del orquestador.
                    _logger.LogError(ex, WorkerMessages.Log.ActivityFailed, watcher.CurrentActivity.SerialNumber);
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            // Total de dispositivos conectados
            _logger.LogInformation(WorkerMessages.Log.TotalConnectedDevices, _activeWatchers.Count);
        }
    }

    /// <summary>
    /// Handler que se activa cuando el <see cref="IDeviceMonitor"/> detecta la desconexión de un dispositivo.
    /// </summary>
    /// <param name="driveLetter">La letra de unidad asignada al dispositivo (ej: "E:").</param>
    private void HandleDeviceDisconnected(string driveLetter)
    {
        _logger.LogInformation(WorkerMessages.Log.DeviceDisconnected, driveLetter);

        if (_activeWatchers.TryRemove(driveLetter, out var watcher))
        {
            // El watcher usa su propio CurrentActivity.FinalAvailableMB y lo sella.
            watcher.FinalizeActivity();
            watcher.Dispose();

            _logger.LogInformation(WorkerMessages.Log.TotalConnectedDevices, _activeWatchers.Count);
        }
    }

    /// <summary>
    /// Handler llamado cuando un <see cref="DeviceActivityWatcher"/> ha recolectado todos los datos al desconectarse.
    /// </summary>
    /// <remarks>
    /// Este método utiliza <see cref="IActivityStorageService"/> que encapsula la lógica de **persistencia resiliente**.
    /// Si la API no está lista, el servicio persistirá automáticamente en SQLite (cumpliendo con OCP/DIP).
    /// </remarks>
    /// <param name="activity">Los datos finales de actividad.</param>
    private async void HandleActivityCompleted(DeviceActivity activity)
    {
        _logger.LogInformation(WorkerMessages.Log.ActivityFinished, activity.SerialNumber, activity.TimeInserted);

        try
        {
            // 1. PERSISTIR LA ACTIVIDAD FINAL (Con sus contadores finales, FilesCopied y FilesDeleted)
            await _devicePersistenceService.PersistActivityAsync(activity);

            // 2. CALCULAR Y PERSISTIR FACTURA
            // Ahora, la actividad está completa y se puede aplicar la regla de no eliminación.
            await _devicePersistenceService.PersistInvoiceAsync(activity, false);

            _logger.LogInformation(WorkerMessages.Log.ActivityInvoiceProcessed, activity.SerialNumber);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, WorkerMessages.Log.ActivityInvoiceProcessedFailed, activity.SerialNumber);
        }
    }

    /// <summary>
    /// Se invoca cuando el host realiza un apagado ordenado. 
    /// Aquí se registra el evento de parada, se detiene el monitoreo WMI y se fuerzan las finalizaciones de los Watchers activos.
    /// </summary>
    /// <inheritdoc path="param[@name='stoppingToken']" />
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(WorkerMessages.Log.ServiceStopping);

        // Registrar evento de parada delegado al servicio de persistencia.
        await _devicePersistenceService.PersistServiceEventAsync(new ServiceEvent
        {
            Timestamp = DateTime.Now,
            EventType = WorkerMessages.ServiceEventTypes.ServiceStop,
            Message = WorkerMessages.ServiceEventMessages.ServiceStopped
        });

        // Desuscribir y detener el monitor principal
        _deviceMonitor.DeviceConnected -= HandleDeviceConnected;
        _deviceMonitor.DeviceDisconnected -= HandleDeviceDisconnected;
        _deviceMonitor.StopMonitoring();

        // Disponer de todos los watchers activos y persistir sus actividades
        foreach (var watcher in _activeWatchers.Values)
        {
            watcher.FinalizeActivity(); // Forzar finalización
            watcher.Dispose();
        }
        _activeWatchers.Clear();

        await base.StopAsync(stoppingToken);
    }
}
