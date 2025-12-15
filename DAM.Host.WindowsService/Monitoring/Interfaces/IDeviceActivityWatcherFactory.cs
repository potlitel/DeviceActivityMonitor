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
        /// <summary>
        /// Crea e inicializa una nueva instancia de IDeviceActivityWatcher.
        /// </summary>
        /// <param name="driveLetter">La letra de unidad específica que debe monitorear el nuevo Watcher (ej: "E:").</param>
        /// <param name="logger">El logger específico para la instancia del DeviceActivityWatcher.</param>
        /// <returns>Una nueva instancia de la abstracción IDeviceActivityWatcher.</returns>
        IDeviceActivityWatcher Create(string driveLetter, ILogger<DeviceActivityWatcher> logger);
    }
}
