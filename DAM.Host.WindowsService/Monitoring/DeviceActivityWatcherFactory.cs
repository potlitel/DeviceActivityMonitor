using DAM.Host.WindowsService.Monitoring.Interfaces;

namespace DAM.Host.WindowsService.Monitoring
{
    public class DeviceActivityWatcherFactory : IDeviceActivityWatcherFactory
    {
        // La implementación solo crea la instancia concreta
        public IDeviceActivityWatcher Create(string driveLetter, ILogger<DeviceActivityWatcher> logger)
        {
            // Retorna la implementación concreta original
            return new DeviceActivityWatcher(driveLetter, logger);
        }
    }
}
