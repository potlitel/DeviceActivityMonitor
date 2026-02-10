//using DAM.Core.Abstractions;
//using DAM.Core.Common;
//using DAM.Core.DTOs.AuditLogs;
//using DAM.Core.Features.Audit.Queries;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace DAM.Infrastructure.Features.AuditLogs
//{
//    //public class GetAuditLogsHandler(IAuditRepository repository) : IQueryHandler<GetAuditLogsQuery, PaginatedList<AuditLogDto>>
//    //{
//    //    public async Task<PaginatedList<AuditLogDto>> HandleAsync(GetAuditLogsQuery q, CancellationToken ct)
//    //        => await repository.GetAllQueryable()
//    //            .OrderByDescending(x => x.TimestampUtc)
//    //            .ToPaginatedListAsync(q.Page, q.Size, x => new AuditLogDto(x.Username, x.Action, x.TimestampUtc), ct);
//    //}
//}
