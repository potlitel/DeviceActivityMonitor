using DAM.Core.Abstractions;
using DAM.Core.DTOs.AuditLogs;
using DAM.Core.Features.Audit.Queries;
using DAM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Infrastructure.Features.AuditLogs
{
    public class GetAuditLogByIdHandler(IAuditRepository repository) : IQueryHandler<GetAuditLogByIdQuery, AuditLogDto?>
    {
        public async Task<AuditLogDto?> HandleAsync(GetAuditLogByIdQuery q, CancellationToken ct)
        {
            var x = await repository.GetByIdAsync(q.Id, ct);
            return x == null ? null : new AuditLogDto(x.Id, x.UserId, x.Action, x.Resource, x.HttpMethod, x.TimestampUtc);
        }
    }
}
