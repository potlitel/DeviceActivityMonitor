using DAM.Core.Settings;
using DAM.Host.WindowsService.Monitoring.Interfaces;
using Microsoft.Extensions.Options;

namespace DAM.Host.WindowsService.Monitoring
{
    /// <summary>
    /// Implementa la interfaz IDeviceActivityWatcherFactory y actúa como una Factoría
    /// concreta para crear instancias del servicio de monitoreo de dispositivos.
    /// 
    /// Esta clase es crucial para aplicar el principio de Inversión de Dependencias (DIP)
    /// y el patrón Factory Method, asegurando que el Worker (código de alto nivel)
    /// dependa de la abstracción (IDeviceActivityWatcherFactory) y no de la clase concreta
    /// (DeviceActivityWatcher).
    /// </summary>
    public class DeviceActivityWatcherFactory : IDeviceActivityWatcherFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptions<StorageSettings> _storageSettings;

        public DeviceActivityWatcherFactory(ILoggerFactory loggerFactory, IOptions<StorageSettings> storageSettings)
        {
            _loggerFactory = loggerFactory;
            _storageSettings = storageSettings;
        }

        /// <inheritdoc/>
        public IDeviceActivityWatcher Create(string driveLetter, ILogger<DeviceActivityWatcher> logger)
        {
            // Retorna la implementación concreta original
            return new DeviceActivityWatcher(
            driveLetter,
            _storageSettings,
            _loggerFactory.CreateLogger<DeviceActivityWatcher>()
        );
        }
    }
}
