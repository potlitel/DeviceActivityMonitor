using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.DevicePresence;
using DAM.Core.Features.Presence.Queries;
using DAM.Core.Interfaces;
using DAM.Core.FilterExtensions;
using DAM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
namespace DAM.Infrastructure.Features.DevicePresence
{
    /// <summary>
    /// Handler para consultar eventos de presencia con paginación.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementa la lógica de negocio para recuperar eventos de presencia paginados,
    /// aplicando filtros por ActivityId cuando se especifica.
    /// </para>
    /// <para>
    /// <b>Optimizaciones:</b>
    /// <list type="bullet">
    /// <item><description>Incluye la entidad DeviceActivity relacionada mediante eager loading</description></item>
    /// <item><description>Usa <c>AsNoTracking()</c> para lecturas eficientes (implícito en el repositorio)</description></item>
    /// <item><description>Proyección selectiva a DTO para minimizar data transfer</description></item>
    /// <item><description>Ordenamiento descendente por timestamp para mostrar eventos más recientes primero</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="repository">Repositorio de presencia inyectado por DI.</param>
    public class GetPresencesHandler(IPresenceRepository repository)
        : IQueryHandler<GetPresencesQuery, PaginatedList<DevicePresenceDto>>
    {
        public async Task<PaginatedList<DevicePresenceDto>> HandleAsync(
            GetPresencesQuery query,
            CancellationToken cancellationToken)
        {
            //https://gemini.google.com/app/ae68da0d5134a995
            //return await repository.GetAllQueryable(query => query.Include(x => x.DeviceActivity))
            //    .ApplyPresenceFilters(query.Filter)
            //    .OrderByDescending(x => x.Timestamp)
            //    .ToPaginatedListAsync(
            //        query.Filter.PageNumber,
            //        query.Filter.PageSize,
            //        entity => new DevicePresenceDto(
            //            entity.Id,
            //            entity.SerialNumber,
            //            entity.Timestamp,
            //            null!
            //        ),
            //        cancellationToken
            //    );

            return await repository.GetAllQueryable(query => query.Include(x => x.DeviceActivity))
                .ApplyPresenceFilters(query.Filter)
                .OrderByDescending(x => x.Timestamp)
                .ToPaginatedListAsync(
                    query.Filter.PageNumber,
                    query.Filter.PageSize,
                    entity => DevicePresenceDto.FromEntity(entity),
                    cancellationToken
                );
        }
    }
}
