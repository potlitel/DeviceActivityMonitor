//using DAM.Core.Abstractions;
//using DAM.Core.Common;
//using DAM.Core.DTOs.Audit;
//using DAM.Core.Features.Audit.Queries;
//using DAM.Infrastructure.Features.Audit;

//namespace DAM.Infrastructure.Caching.Decorators;

//public class CachedGetAuditLogsHandler(
//    GetAuditLogsHandler innerHandler,
//    ICacheService cache) : IQueryHandler<GetAuditLogsQuery, PaginatedList<AuditLogResponse>>
//{
//    public async Task<PaginatedList<AuditLogResponse>> HandleAsync(GetAuditLogsQuery query, CancellationToken ct)
//    {
//        // Generamos una llave única basada en los filtros de la query
//        string cacheKey = $"audit_logs_{query.Filter.PageNumber}_{query.Filter.PageSize}_{query.Filter.Username}";

//        var cachedData = cache.GetAsync<PaginatedList<AuditLogResponse>>(cacheKey);
//        if (cachedData != null) return cachedData;

//        // Si no está, ejecutamos el handler original (base de datos)
//        var result = await innerHandler.HandleAsync(query, ct);

//        // Guardamos por 1 minuto (los logs no necesitan tiempo real absoluto)
//        await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(1));

//        return result;
//    }
//}