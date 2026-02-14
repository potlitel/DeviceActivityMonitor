using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.DevicePresence;
using DAM.Core.Features.Presence.Queries;
using DAM.Core.Interfaces;
using DAM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
namespace DAM.Infrastructure.Features.DevicePresence
{
    /// <summary>
    /// Handler para consultar eventos de presencia con paginación.
    /// </summary>
    public class GetPresencesHandler(IPresenceRepository repository)
        : IQueryHandler<GetPresencesQuery, PaginatedList<DevicePresenceDto>>
    {
        public async Task<PaginatedList<DevicePresenceDto>> HandleAsync(
            GetPresencesQuery query,
            CancellationToken cancellationToken)
        {
            return await repository.GetAllQueryable(query => query.Include(x => x.DeviceActivity))
                .OrderByDescending(x => x.Timestamp)
                .ToPaginatedListAsync(
                    query.Filter.PageNumber,
                    query.Filter.PageSize,
                    entity => new DevicePresenceDto(
                        entity.Id,
                        entity.SerialNumber,
                        entity.Timestamp,
                        null
                    ),
                    cancellationToken
                );
        }
    }
}
