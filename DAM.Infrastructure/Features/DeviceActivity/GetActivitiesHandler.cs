using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.DeviceActivity;
using DAM.Core.Features.Activities.Queries;
using DAM.Core.FilterExtensions;
using DAM.Core.Interfaces;
using DAM.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

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
    public class GetActivitiesHandler(IActivityRepository repository, ILogger<GetActivitiesHandler> logger)
    : IQueryHandler<GetActivitiesQuery, PaginatedList<DeviceActivityDto>>
    {
        /// <inheritdoc/>
        public async Task<PaginatedList<DeviceActivityDto>> HandleAsync(
            GetActivitiesQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                logger.LogDebug("Obteniendo actividades con filtros: {@Filters}", query.Filter);

                // 1. Aplicamos filtros y ordenamiento.
                var filteredQuery = repository.GetAllQueryable()
                    .ApplyActivityFilters(query.Filter)
                    .OrderByDescending(x => x.InsertedAt);

                // 2. Proyectamos a DTO
                var projectedQuery = filteredQuery.ToListDto();

                // 3. Usamos ToPaginatedListAsync con una función identidad
                //    porque projectedQuery ya es IQueryable<DeviceActivityDto>
                return await projectedQuery.ToPaginatedListAsync(
                    query.Filter.PageNumber,
                    query.Filter.PageSize,
                    dto => dto,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener actividades");
                throw;
            }
        }
    }
}
