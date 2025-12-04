using DAM.Core.Entities;
using DAM.Core.Interfaces;
using DAM.Host.WindowsService.Monitoring;
using System.Collections.Concurrent;

namespace DAM.Host.WindowsService;

public class Worker(ILogger<Worker> logger, IDeviceMonitor deviceMonitor, ILoggerFactory loggerFactory) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IDeviceMonitor _deviceMonitor = deviceMonitor;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    // Diccionario para mantener un Watcher por cada dispositivo activo.
    private readonly ConcurrentDictionary<string, DeviceActivityWatcher> _activeWatchers = new();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DAM Worker Service starting at: {time}", DateTimeOffset.Now);

        // Suscribirse a eventos de conexión/desconexión
        _deviceMonitor.DeviceConnected += HandleDeviceConnected;
        _deviceMonitor.DeviceDisconnected += HandleDeviceDisconnected;
        _deviceMonitor.StartMonitoring();

        _logger.LogInformation("Device Monitoring Started.");

        // Este servicio solo es un host, la lógica de trabajo está en los handlers.
        // No necesita un bucle de trabajo (while(!stoppingToken.IsCancellationRequested)) 
        // a menos que necesite tareas de mantenimiento periódicas.
        return Task.CompletedTask;
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
            long finalAvailableMB = 0;
            try
            {
                var driveInfo = new DriveInfo(driveLetter);
                if (driveInfo.IsReady)
                {
                    finalAvailableMB = driveInfo.AvailableFreeSpace / (1024 * 1024);
                }
            }
            catch { /* El disco ya puede no estar accesible */ }

            // Finalizar el Watcher y reportar datos
            watcher.FinalizeActivity(finalAvailableMB);
            watcher.Dispose();

            _logger.LogInformation("Total connected devices: {Count}", _activeWatchers.Count);
        }
    }

    // Handler para cuando el Watcher tiene los datos finales
    private void HandleActivityCompleted(DeviceActivity activity)
    {
        _logger.LogInformation("Activity finished for {SN}. Time: {Time}", activity.SerialNumber, activity.TimeInserted);

        // **AQUÍ VA LA LÓGICA DE PERSISTENCIA (BD o API)**
        // Este mecanismo lo implementaremos en la siguiente fase.

        // Ejemplo de métricas:
        // long diff = activity.InitialAvailableMB - activity.FinalAvailableMB;
        // _logger.LogInformation("Diff MB: {Diff}", diff); 
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DAM Worker Service is stopping.");

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
