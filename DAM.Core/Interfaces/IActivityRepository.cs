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
        /// Registra una nueva actividad de dispositivo en el contexto de persistencia.
        /// </summary>
        /// <param name="activity">El objeto DeviceActivity a registrar.</param>
        /// <returns>Tarea asíncrona que representa la operación de registro en el contexto.</returns>
        Task AddActivityAsync(DeviceActivity activity);

        /// <summary>
        /// Marca una actividad existente como modificada en el contexto.
        /// </summary>
        /// <param name="activity">El objeto DeviceActivity a actualizar.</param>
        /// <returns>Tarea asíncrona que representa la operación de registro en el contexto.</returns>
        Task UpdateActivityAsync(DeviceActivity activity);

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

        /// <summary>
        /// Recupera todas las actividades que tienen archivos copiados pero carecen de una factura asociada.
        /// </summary>
        /// <returns>Una colección de actividades "huérfanas" o pendientes de procesamiento.</returns>
        Task<IEnumerable<DeviceActivity>> GetActivitiesMissingInvoicesAsync();
    }
}
