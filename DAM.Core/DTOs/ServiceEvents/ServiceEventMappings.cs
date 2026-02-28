using DAM.Core.Entities;
using DAM.Core.Features.ServiceEvents.Commands;

namespace DAM.Core.DTOs.ServiceEvents
{
    /// <summary>
    /// Proporciona métodos de extensión para mapear comandos de eventos de servicio a entidades <see cref="ServiceEvent"/>.
    /// </summary>
    /// <remarks>
    /// Los eventos de servicio se utilizan para auditoría y monitoreo del funcionamiento interno
    /// del worker service de Windows.
    /// </remarks>
    public static class ServiceEventMappings
    {
        /// <summary>
        /// Convierte un comando de evento de servicio en una entidad <see cref="ServiceEvent"/>.
        /// </summary>
        /// <param name="cmd">El comando con los datos del evento a registrar.</param>
        /// <returns>Una nueva instancia de <see cref="ServiceEvent"/> lista para persistir.</returns>
        /// <remarks>
        /// Los eventos de servicio son inmutables y representan puntos de control en el ciclo de vida
        /// del worker service: inicios, paradas normales, intentos de detención manual, etc.
        /// <para>
        /// El campo <see cref="ServiceEvent.EventType"/> debe contener valores normalizados como:
        /// "START", "STOP", "MANUAL_STOP_ATTEMPT", "HEALTH_CHECK", etc.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Se lanza si <paramref name="cmd"/> es null.</exception>
        public static ServiceEvent ToEntity(this CreateServiceEventCmd cmd)
        {
            return new ServiceEvent
            {
                Timestamp = cmd.Timestamp,
                EventType = cmd.EventType,
                Message = cmd.Message
            };
        }
    }
}
