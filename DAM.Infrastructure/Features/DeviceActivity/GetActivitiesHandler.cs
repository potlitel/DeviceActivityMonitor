using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.DeviceActivity;
using DAM.Core.Features.Activities.Queries;
using DAM.Core.Interfaces;
using DAM.Infrastructure.Extensions;

namespace DAM.Infrastructure.Features.DeviceActivity
{
    /// <summary>
    /// Handler para consultar actividades de dispositivos con paginación.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementa la lógica de negocio para recuperar actividades paginadas,
    /// aplicando ordenamiento descendente por fecha de inserción para mostrar
    /// las actividades más recientes primero.
    /// </para>
    /// <para>
    /// <b>Optimizaciones:</b>
    /// <list type="bullet">
    /// <item><description>Usa <c>AsNoTracking()</c> para lecturas eficientes</description></item>
    /// <item><description>Proyección selectiva a DTO para minimizar data transfer</description></item>
    /// <item><description>Ejecución diferida hasta la paginación</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="repository">Repositorio de actividades inyectado por DI.</param>
    public class GetActivitiesHandler(IActivityRepository repository)
    : IQueryHandler<GetActivitiesQuery, PaginatedList<DeviceActivityDto>>
    {
        /// <inheritdoc/>
        public async Task<PaginatedList<DeviceActivityDto>> HandleAsync(
            GetActivitiesQuery query,
            CancellationToken cancellationToken)
        {
            return await repository.GetAllQueryable()
                .OrderByDescending(x => x.InsertedAt)
                .ToPaginatedListAsync(
                    query.Filter.PageNumber,
                    query.Filter.PageSize,
                    entity => new DeviceActivityDto(
                        entity.Id,
                        entity.SerialNumber,
                        entity.Model,
                        entity.TotalCapacityMB,
                        entity.InsertedAt,
                        entity.ExtractedAt,
                        entity.InitialAvailableMB,
                        entity.FinalAvailableMB
                    ),
                    cancellationToken
                );
        }
    }
}
