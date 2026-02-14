using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.Audit;
using DAM.Core.Features.Audit.Queries;
using DAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DAM.Infrastructure.Features.Audit;

//public class GetAuditLogsHandler(DeviceActivityDbContext db) : IQueryHandler<GetAuditLogsQuery, PaginatedList<AuditLogResponse>>
//{
//    public async Task<PaginatedList<AuditLogResponse>> HandleAsync(GetAuditLogsQuery query, CancellationToken ct)
//    {
//        var f = query.Filter;

//        // IQueryable para filtros dinámicos (Principio 8)
//        var baseQuery = db.AuditLogs.AsNoTracking().AsQueryable();

//        if (!string.IsNullOrEmpty(f.Username))
//            baseQuery = baseQuery.Where(x => x.Username.Contains(f.Username));

//        if (f.FromDate.HasValue)
//            baseQuery = baseQuery.Where(x => x.TimestampUtc >= f.FromDate.Value);

//        // Conteo total para paginación
//        var totalCount = await baseQuery.CountAsync(ct);

//        // Paginación y Mapeo manual (DTOs Exclusivos)
//        var items = await baseQuery
//            .OrderByDescending(x => x.TimestampUtc)
//            .Skip((f.PageNumber - 1) * f.PageSize)
//            .Take(f.PageSize)
//            .Select(x => new AuditLogResponse(
//                x.Id,
//                x.Username,
//                x.Action,
//                x.Resource,
//                x.HttpMethod,
//                x.IpAddress,
//                x.TimestampUtc
//            ))
//            .ToListAsync(ct);

//        return new PaginatedList<AuditLogResponse>(items, totalCount, f.PageNumber, f.PageSize);
//    }
//}