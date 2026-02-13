using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.Events;
using DAM.Core.Features.Events.Queries;
using DAM.Core.Interfaces;
using DAM.Infrastructure.Extensions;

namespace DAM.Infrastructure.Features.ServiceEvents
{
    public class GetServiceEventsHandler(IServiceEventRepository repository) : IQueryHandler<GetServiceEventsQuery, PaginatedList<ServiceEventDto>>
    {
        public async Task<PaginatedList<ServiceEventDto>> HandleAsync(GetServiceEventsQuery q, CancellationToken ct)
            => await repository.GetAllQueryable()
                .OrderByDescending(x => x.Timestamp)
                .ToPaginatedListAsync(q.Filter.PageNumber, q.Filter.PageSize, x => new ServiceEventDto(x.Id, x.Message), ct);
    }
}
