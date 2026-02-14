using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.Events;
using DAM.Core.Features.Events.Queries;
using DAM.Infrastructure.Extensions;
using DAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class GetServiceEventsHandler(DeviceActivityDbContext db)
    : IQueryHandler<GetServiceEventsQuery, PaginatedList<ServiceEventDto>>
{
    public async Task<PaginatedList<ServiceEventDto>> HandleAsync(GetServiceEventsQuery query, CancellationToken ct)
    {
        var f = query.Filter;
        var q = db.ServiceEvents.AsNoTracking().AsQueryable();

        // Filtros dinámicos
        //if (f.Level.HasValue)
        //    q = q.Where(x => x.Level == f.Level.Value);

        //if (!string.IsNullOrEmpty(f.Source))
        //    q = q.Where(x => x.Source.Contains(f.Source));

        //return await q.OrderByDescending(x => x.TimestampUtc)
        //              .ToPaginatedListAsync(f.PageNumber, f.PageSize, x => new ServiceEventDto(
        //                  x.Id,
        //                  x.Level.ToString(),
        //                  x.Source,
        //                  x.Message,
        //                  x.TimestampUtc
        //              ), ct);

        return await q.OrderByDescending(x => x.Timestamp)
                      .ToPaginatedListAsync(f.PageNumber, f.PageSize, x => new ServiceEventDto(
                          x.Id,
                          x.Message
                      ), ct);
    }
}