using DAM.Core.Abstractions;
using DAM.Core.DTOs.DeviceActivity;
using DAM.Core.Features.Activities.Queries;
using DAM.Core.Interfaces;

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
    public class GetActivityByIdHandler(IActivityRepository repository)
        : IQueryHandler<GetActivityByIdQuery, DeviceActivityDto?>
    {
        /// <inheritdoc/>
        public async Task<DeviceActivityDto?> HandleAsync(
            GetActivityByIdQuery query,
            CancellationToken cancellationToken)
        {
            var entity = await repository.GetByIdAsync(query.Id, cancellationToken);

            if (entity == null)
                return null;

            return new DeviceActivityDto(
                entity.Id,
                entity.SerialNumber,
                entity.Model,
                entity.TotalCapacityMB,
                entity.InsertedAt,
                entity.ExtractedAt,
                entity.InitialAvailableMB,
                entity.FinalAvailableMB
            );
        }
    }
}
