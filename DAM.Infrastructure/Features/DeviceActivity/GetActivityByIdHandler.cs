using DAM.Core.Abstractions;
using DAM.Core.DTOs.DeviceActivity;
using DAM.Core.Features.Activities.Queries;
using DAM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAM.Infrastructure.Features.DeviceActivity
{
    /// <summary>
    /// Handler para obtener una actividad específica por su identificador GUID.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Retorna null si no se encuentra la actividad, permitiendo al caller
    /// decidir si lanzar una excepción o manejar el caso de "no encontrado".
    /// </para>
    /// <para>
    /// <b>Patrón Null Object:</b> Considerar retornar un DTO vacío en lugar de null
    /// para simplificar el manejo en clientes.
    /// </para>
    /// </remarks>
    /// <param name="repository">Repositorio de actividades.</param>
    public class GetActivityByIdHandler(IActivityRepository repository, ILogger<GetActivityByIdHandler> logger)
        : IQueryHandler<GetActivityByIdQuery, DeviceActivityDto?>
    {
        /// <inheritdoc/>
        public async Task<DeviceActivityDto?> HandleAsync(
            GetActivityByIdQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                logger.LogDebug("Obteniendo actividad por ID: {ActivityId}", query.Id);

                // 🎯 Usamos el método genérico con includes para cargar relaciones
                var entity = await repository.GetByIdAsync(
                    query.Id,
                    cancellationToken,
                    // Includes para cargar colecciones relacionadas
                    q => q.Include(a => a.PresenceHistory),
                    q => q.Include(a => a.Invoices)
                );

                if (entity == null)
                {
                    logger.LogWarning("No se encontró actividad con ID: {ActivityId}", query.Id);
                    return null;
                }

                // Mapeamos la entidad a DTO usando FromEntity
                return DeviceActivityDto.FromEntity(entity);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener actividad por ID: {ActivityId}", query.Id);
                throw;
            }
        }
    }
}
