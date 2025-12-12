using DAM.Core.Entities;
using DAM.Core.Interfaces;
using DAM.Host.WindowsService.Monitoring;
using System.Collections.Concurrent;

namespace DAM.Host.WindowsService;

/// <summary>
/// Servicio principal de Windows que orquesta la detección de dispositivos y el ciclo de vida de los Watchers.
/// </summary>
/// <remarks>
/// Hereda de <see cref="BackgroundService"/> para ejecutarse como un servicio de larga duración.
/// </remarks>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IDeviceMonitor _deviceMonitor;
    private readonly ILoggerFactory _loggerFactory;
    //private readonly IActivityStorageService _storageService;
    private readonly IServiceScopeFactory _scopeFactory;
    /// <summary>
    /// Colección concurrente para mantener un <see cref="DeviceActivityWatcher"/> activo por cada dispositivo conectado.
    /// </summary>
    private readonly ConcurrentDictionary<string, DeviceActivityWatcher> _activeWatchers = new();

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="Worker"/>.
    /// </summary>
    /// <param name="logger">Servicio de logging.</param>
    /// <param name="deviceMonitor">Monitor de eventos de hardware (WMI).</param>
    /// <param name="storageService">Servicio resiliente para la persistencia de datos (<see cref="IActivityStorageService"/>).</param>
    public Worker(ILogger<Worker> logger, IDeviceMonitor deviceMonitor, ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _deviceMonitor = deviceMonitor;
        _loggerFactory = loggerFactory;
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DAM Worker Service starting at: {time}", DateTimeOffset.Now);

        // 1. Registrar evento de inicio en la base de datos (Persistencia de Eventos)
        using (var scope = _scopeFactory.CreateScope())
        {
            var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();
            await storageService.StoreServiceEventAsync(new ServiceEvent
            {
                Timestamp = DateTime.Now,
                EventType = "SERVICE_START",
                Message = "El servicio Device Activity Monitor ha iniciado la ejecución."
            });
        }

        // 2. Configurar y arrancar el monitoreo WMI
        // Suscribirse a eventos de conexión/desconexión
        _deviceMonitor.DeviceConnected += HandleDeviceConnected;
        _deviceMonitor.DeviceDisconnected += HandleDeviceDisconnected;
        _deviceMonitor.StartMonitoring();

        _logger.LogInformation("Device Monitoring Started.");

        // Este servicio solo es un host, la lógica de trabajo está en los handlers.
        // No necesita un bucle de trabajo (while(!stoppingToken.IsCancellationRequested)) 
        // a menos que necesite tareas de mantenimiento periódicas.
        //return Task.CompletedTask;
        // El Worker se mantiene en ejecución hasta que se solicita la detención.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    // --- Manejo de Eventos ---

    private void HandleDeviceConnected(string driveLetter)
    {
        _logger.LogInformation("Device connected: {DriveLetter}", driveLetter);

        // Aseguramos que solo haya un watcher por unidad
        if (!_activeWatchers.ContainsKey(driveLetter))
        {
            // Crear e inicializar un nuevo Watcher
            var watcher = new DeviceActivityWatcher(driveLetter, _loggerFactory.CreateLogger<DeviceActivityWatcher>());
            watcher.ActivityCompleted += HandleActivityCompleted;
            _activeWatchers.TryAdd(driveLetter, watcher);

            // Total de dispositivos conectados
            _logger.LogInformation("Total connected devices: {Count}", _activeWatchers.Count);
        }
    }

    private void HandleDeviceDisconnected(string driveLetter)
    {
        _logger.LogInformation("Device disconnected: {DriveLetter}", driveLetter);

        if (_activeWatchers.TryRemove(driveLetter, out var watcher))
        {
            // Recolectar la capacidad final antes de finalizar
            //long finalAvailableMB = 0;
            //try
            //{
            //    var driveInfo = new DriveInfo(driveLetter);
            //    if (!driveInfo.IsReady)
            //    {
            //        finalAvailableMB = driveInfo.AvailableFreeSpace / (1024 * 1024);
            //    }
            //}
            //catch { /* El disco ya puede no estar accesible */ }

            // Finalizar el Watcher y reportar datos

            //watcher.FinalizeActivity(finalAvailableMB);
            watcher.FinalizeActivity(watcher.CurrentActivity.FinalAvailableMB);
            watcher.Dispose();

            _logger.LogInformation("Total connected devices: {Count}", _activeWatchers.Count);
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
        //_logger.LogInformation("Activity finished for {SN}. Time: {Time}", activity.SerialNumber, activity.TimeInserted);

        //// **AQUÍ VA LA LÓGICA DE PERSISTENCIA (BD o API)**
        //// Este mecanismo lo implementaremos en la siguiente fase.

        //// Ejemplo de métricas:
        //// long diff = activity.InitialAvailableMB - activity.FinalAvailableMB;
        //// _logger.LogInformation("Diff MB: {Diff}", diff); 
        //// Persistir la actividad usando el servicio resiliente (API o BD Local)
        //try
        //{
        //    await _storageService.StoreActivityAsync(activity);
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "Fallo crítico al persistir la actividad del dispositivo {SN}.", activity.SerialNumber);
        //}
        _logger.LogInformation("Activity finished for {SN}. Time: {Time}", activity.SerialNumber, activity.TimeInserted);

        // 1. Crear un ámbito (scope) desechable para esta transacción
        using (var scope = _scopeFactory.CreateScope())
        {
            // 2. Obtener el servicio Scoped (StorageService) dentro de este ámbito
            var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();

            try
            {
                // 3. Usar el servicio Scoped
                await storageService.StoreActivityAsync(activity);

                _logger.LogInformation("Actividad del dispositivo {SN} persistida exitosamente.", activity.SerialNumber);
            }
            catch (Exception ex)
            {
                // 4. Si hay error, el log es crítico
                _logger.LogCritical(ex, "FALLO CRÍTICO: No se pudo persistir la actividad del dispositivo {SN}.", activity.SerialNumber);
            }
        } // 5. El ámbito y sus servicios Scoped (incluyendo DbContext) se desechan aquí.
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DAM Worker Service is stopping.");

        // Registrar evento de parada
        using (var scope = _scopeFactory.CreateScope())
        {
            var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();
            await storageService.StoreServiceEventAsync(new ServiceEvent
            {
                Timestamp = DateTime.Now,
                EventType = "SERVICE_STOP",
                Message = "El servicio Device Activity Monitor ha finalizado la ejecución."
            });
        }

        // Desuscribir y detener el monitor principal
        _deviceMonitor.DeviceConnected -= HandleDeviceConnected;
        _deviceMonitor.DeviceDisconnected -= HandleDeviceDisconnected;
        _deviceMonitor.StopMonitoring();

        // Disponer de todos los watchers activos y persistir sus actividades
        foreach (var watcher in _activeWatchers.Values)
        {
            watcher.FinalizeActivity(0); // Forzar finalización
            watcher.Dispose();
        }
        _activeWatchers.Clear();

        await base.StopAsync(stoppingToken);
    }
}
