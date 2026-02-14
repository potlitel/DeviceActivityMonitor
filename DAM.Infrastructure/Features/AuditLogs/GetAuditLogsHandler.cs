using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.AuditLogs;
using DAM.Core.Features.Audit.Queries;
using DAM.Core.Interfaces;
using DAM.Infrastructure.Extensions;

namespace DAM.Infrastructure.Features.AuditLogs
{
    public class GetAuditLogsHandler(IAuditRepository repository) : IQueryHandler<GetAuditLogsQuery, PaginatedList<AuditLogDto>>
    {
        public async Task<PaginatedList<AuditLogDto>> HandleAsync(GetAuditLogsQuery q, CancellationToken ct)
            => await repository.GetAllQueryable()
                .OrderByDescending(x => x.TimestampUtc)
                .ToPaginatedListAsync(q.Filter.PageNumber, q.Filter.PageSize, 
                                      x => new AuditLogDto(x.Id, x.UserId, x.Action, x.Resource, x.HttpMethod, x.TimestampUtc), ct);
    }
}
