using DAM.Core.DTOs.Heartbeat;
using DAM.Core.Entities;

namespace DAM.Core.Interfaces
{
    /// <summary>
    /// Define los servicios de persistencia para el ciclo de vida de los dispositivos y actividades del sistema.
    /// Esta interfaz actúa como la fachada de acceso a datos, garantizando la atomicidad en operaciones complejas.
    /// </summary>
    public interface IDevicePersistenceService
    {
        /// <summary>
        /// Persiste un registro de presencia de dispositivo (conexión) en la base de datos.
        /// </summary>
        /// <param name="serialNumber">El número de serie único del dispositivo físico detectado.</param>
        /// <returns>Una tarea que representa la operación de guardado asíncrono.</returns>
        /// <remarks>
        /// Esta operación es asíncrona y debe ser tratada de forma transaccional para evitar duplicados 
        /// en ráfagas de conexión/desconexión rápidas.
        /// </remarks>
        Task PersistPresenceAsync(DeviceActivity activity);

        /// <summary>
        /// Almacena una actividad completa de un dispositivo, incluyendo archivos copiados y borrados.
        /// </summary>
        /// <param name="activity">Objeto <see cref="DeviceActivity"/> que contiene los metadatos y contadores recolectados.</param>
        /// <returns>Una tarea que representa la persistencia de la actividad.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si el objeto de actividad es nulo.</exception>
        /// <remarks>
        /// Implementa el patrón de Unidad de Trabajo para asegurar que los registros de actividad 
        /// y sus listas relacionadas (FilesCopied, FilesDeleted) se guarden como una operación atómica.
        /// </remarks>
        Task PersistActivityAsync(DeviceActivity activity);

        /// <summary>
        /// Registra un evento de auditoría sobre el estado del Worker Service (Inicio, Parada, Error Crítico).
        /// </summary>
        /// <param name="serviceEvent">Entidad que describe el tipo de evento y el timestamp del sistema.</param>
        /// <returns>Una tarea que representa la inserción en el historial de auditoría.</returns>
        Task PersistServiceEventAsync(ServiceEvent serviceEvent);

        /// <summary>
        /// Procesa la lógica de negocio para generar una factura a partir de una actividad y la persiste.
        /// </summary>
        /// <param name="activity">La actividad finalizada sobre la cual se realizará el cálculo de cobro.</param>
        /// <returns>Una tarea que representa la creación y guardado de la factura.</returns>
        /// <remarks>
        /// Este método debe coordinar el uso de un <c>IInvoiceCalculator</c> interno. 
        /// La operación debe fallar por completo si el cálculo es inválido o si la inserción en BD falla 
        /// (Atomicidad total).
        /// </remarks>
        Task PersistInvoiceAsync(DeviceActivity activity, bool uptHorphanActvs);

        /// <summary>
        /// Escanea la base de datos en busca de actividades que quedaron en estado pendiente (ej: por apagado repentino) 
        /// y procesa su facturación pendiente.
        /// </summary>
        /// <returns>Una tarea que representa el proceso de recuperación.</returns>
        Task RecoverPendingActivitiesAsync();

        Task SendHeartbeatAsync(HeartbeatDto heartbeat);
    }
}
