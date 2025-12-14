using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Host.WindowsService.Monitoring.Interfaces
{
    /// <summary>
    /// Contrato para la creación de instancias de DeviceActivityWatcher.
    /// Esto permite al Worker inyectar una fábrica mockeable en las pruebas.
    /// </summary>
    public interface IDeviceActivityWatcherFactory
    {
        IDeviceActivityWatcher Create(string driveLetter, ILogger<DeviceActivityWatcher> logger);
    }
}
