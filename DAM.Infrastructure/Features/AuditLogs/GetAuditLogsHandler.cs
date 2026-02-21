using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.AuditLogs;
using DAM.Core.Features.Audit.Queries;
using DAM.Core.Interfaces;
using DAM.Core.FilterExtensions;
using DAM.Infrastructure.Extensions;

namespace DAM.Infrastructure.Features.AuditLogs
{
    /// <summary>
    /// Handler para consultar registros de auditoría con paginación.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementa la lógica de negocio para recuperar registros de auditoría paginados,
    /// aplicando filtros por usuario y rango de fechas según los criterios especificados.
    /// </para>
    /// <para>
    /// <b>Optimizaciones:</b>
    /// <list type="bullet">
    /// <item><description>Usa <c>AsNoTracking()</c> para lecturas eficientes (implícito en el repositorio)</description></item>
    /// <item><description>Proyección selectiva a DTO para minimizar data transfer</description></item>
    /// <item><description>Ordenamiento descendente por timestamp para mostrar eventos más recientes primero</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="repository">Repositorio de auditoría inyectado por DI.</param>
    public class GetAuditLogsHandler(IAuditRepository repository)
        : IQueryHandler<GetAuditLogsQuery, PaginatedList<AuditLogDto>>
    {
        /// <inheritdoc/>
        public async Task<PaginatedList<AuditLogDto>> HandleAsync(
            GetAuditLogsQuery query,
            CancellationToken cancellationToken)
        {
            return await repository.GetAllQueryable()
                .ApplyAuditFilters(query.Filter)
                .OrderByDescending(x => x.TimestampUtc)
                .ToPaginatedListAsync(
                    query.Filter.PageNumber,
                    query.Filter.PageSize,
                    entity => new AuditLogDto(
                        entity.Id,
                        entity.Username,
                        entity.Action,
                        entity.Resource,
                        entity.HttpMethod,
                        entity.TimestampUtc
                    ),
                    cancellationToken
                );
        }
    }
}
