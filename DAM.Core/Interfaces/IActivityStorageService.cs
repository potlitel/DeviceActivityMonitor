using DAM.Core.Entities;

namespace DAM.Core.Interfaces
{
    /// <summary>
    /// Contrato para el servicio de almacenamiento de actividad, independientemente de si el destino es local (BD) o remoto (API).
    /// </summary>
    /// <remarks>
    /// Este contrato es clave para implementar el patrón Strategy y el diseño resiliente de almacenamiento.
    /// </remarks>
    public interface IActivityStorageService
    {
        /// <summary>
        /// Almacena una actividad de dispositivo, decidiendo la estrategia de persistencia (Local/Remoto).
        /// </summary>
        /// <param name="activity">La actividad del dispositivo a almacenar.</param>
        /// <returns>Tarea asíncrona completada.</returns>
        Task StoreActivityAsync(DeviceActivity activity);

        /// <summary>
        /// Almacena un evento del servicio, decidiendo la estrategia de persistencia (Local/Remoto).
        /// </summary>
        /// <param name="serviceEvent">El evento del servicio a almacenar.</param>
        /// <returns>Tarea asíncrona completada.</returns>
        Task StoreServiceEventAsync(ServiceEvent serviceEvent);

        /// <summary>
        /// Almacena un historial de presencia del dispositivo, decidiendo la estrategia de persistencia (Local/Remoto).
        /// </summary>
        /// <param name="presence">El historial de presencia a almacenar.</param>
        /// <returns>Tarea asíncrona completada.</returns>
        Task StoreDevicePresenceAsync(DevicePresence presence);

        /// <summary>
        /// Almacena una factura de operación de copiado de un dispositivo, decidiendo la estrategia de persistencia (Local/Remoto).
        /// </summary>
        /// <param name="invoice">La factura a almacenar.</param>
        /// <returns>Tarea asíncrona completada.</returns>
        Task StoreInvoiceAsync(Invoice invoice);
    }
}
