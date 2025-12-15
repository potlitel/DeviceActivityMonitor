using DAM.Core.Entities;

namespace DAM.Core.Interfaces
{
    /// <summary>
    /// Contrato para la manipulación y persistencia de las entidades de actividad en la base de datos.
    /// </summary>
    /// <remarks>
    /// Este repositorio actúa como una abstracción de la capa de datos.
    /// </remarks>
    public interface IActivityRepository
    {
        /// <summary>
        /// Agrega una nueva actividad de dispositivo a la persistencia.
        /// </summary>
        /// <param name="activity">El objeto DeviceActivity a guardar.</param>
        /// <returns>Tarea asíncrona completada.</returns>
        Task AddActivityAsync(DeviceActivity activity);

        /// <summary>
        /// Agrega un evento del servicio a la persistencia.
        /// </summary>
        /// <param name="serviceEvent">El objeto ServiceEvent a guardar.</param>
        /// <returns>Tarea asíncrona completada.</returns>
        Task AddServiceEventAsync(ServiceEvent serviceEvent);

        /// <summary>
        /// Agrega un historial de presencia de dispositivo a la persistencia.
        /// </summary>
        /// <param name="presence">El objeto DevicePresence a guardar.</param>
        /// <returns>Tarea asíncrona completada.</returns>
        Task AddDevicePresenceAsync(DevicePresence presence);

        /// <summary>
        /// Agrega una factura de una operación de copiado en dispositivo a la persistencia.
        /// </summary>
        /// <param name="invoice">El objeto Invoice a guardar.</param>
        /// <returns>Tarea asíncrona completada.</returns>
        Task AddInvoiceAsync(Invoice invoice);

        // ... otros métodos de consulta ...
    }
}
