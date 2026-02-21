using DAM.Api.Base;
using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.Audit;
using DAM.Core.DTOs.AuditLogs;
using DAM.Core.Features.Audit.Queries;
using DAM.Core.Features.Presence.Queries;
using DAM.Infrastructure.CQRS;
using FastEndpoints;

namespace DAM.Api.Features.Audit;

//public record GetAuditLogsQuery(AuditLogFilter Filter) : IQuery<PaginatedList<AuditLogResponse>>;

/// <summary>
/// 📋 Obtiene el historial completo de auditoría del sistema con capacidades avanzadas de filtrado.
/// </summary>
/// <remarks>
/// <para>
/// <b>🔍 Detalles del endpoint:</b>
/// <list type="bullet">
/// <item><description><b>Método:</b> GET</description></item>
/// <item><description><b>Ruta:</b> /audit/logs</description></item>
/// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
/// <item><description><b>Roles permitidos:</b> Manager</description></item>
/// </list>
/// </para>
/// <para>
/// <b>🎯 Propósito:</b>
/// Proporciona una vista completa y filtrable de todas las operaciones realizadas en el sistema.
/// Es la herramienta principal para:
/// <list type="bullet">
/// <item><description>👁️‍🗨️ Supervisión de actividades de usuarios</description></item>
/// <item><description>🔍 Investigación de incidentes de seguridad</description></item>
/// <item><description>📊 Generación de reportes de cumplimiento</description></item>
/// <item><description>🧪 Auditoría forense y trazabilidad</description></item>
/// </list>
/// </para>
/// <para>
/// <b>📌 Filtros disponibles:</b>
/// | Parámetro | Tipo | Descripción |
/// |-----------|------|-------------|
/// | Username | string | Filtra por nombre de usuario (contiene) |
/// | FromDate | datetime | Registros posteriores a esta fecha |
/// | ToDate | datetime | Registros anteriores a esta fecha |
/// | Action | string | Tipo de acción (Login, Create, Update, Delete) |
/// | Resource | string | Recurso afectado |
/// | PageNumber | int | Número de página (≥1) |
/// | PageSize | int | Registros por página (1-100) |
/// </list>
/// </para>
/// <para>
/// <b>⚠️ Consideraciones de rendimiento:</b>
/// Para rangos de fechas muy amplios, se recomienda utilizar paginación y
/// limitar el tamaño de página a 50 registros o menos.
/// </para>
/// </remarks>
/// <response code="200">✅ Lista paginada de registros de auditoría</response>
/// <response code="400">❌ Parámetros de filtrado inválidos</response>
/// <response code="401">❌ No autenticado o token inválido</response>
/// <response code="403">❌ No autorizado - Se requiere rol 'Manager'</response>
public class GetAuditLogsEndpoint : BaseEndpoint<AuditLogFilter, PaginatedList<AuditLogDto>>
{
    private readonly IDispatcher _dispatcher;

    public GetAuditLogsEndpoint(IDispatcher dispatcher) => _dispatcher = dispatcher;

    public override void Configure()
    {
        Get("/audit/logs");
        Roles("Manager");

        Description(x => x
            .Produces<ApiResponse<PaginatedList<AuditLogResponse>>>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .WithTags("📋 Auditoría")
            .WithDescription("""
                Este endpoint implementa paginación tipo offset (PageNumber/PageSize).
                
                🔍 **Ejemplo de consulta:**
                ```
                GET /audit/logs?Username=admin&FromDate=2026-01-01&PageNumber=1&PageSize=20
                ```
                """));

        Summary(s =>
        {
            s.Summary = "📋 [Auditoría] Obtiene historial completo con filtros";
            s.Description = """
                Recupera el historial de auditoría del sistema con múltiples criterios de filtrado.
                
                **💡 Pro-tip:** Use fechas en formato ISO 8601 (yyyy-MM-dd) para mejor compatibilidad.
                """;
            s.ExampleRequest = new AuditLogFilter
            {
                Username = "admin",
                FromDate = DateTime.UtcNow.AddDays(-7),
                ToDate = DateTime.UtcNow,
                PageNumber = 1,
                PageSize = 20
            };
        });
    }

    public override async Task HandleAsync(AuditLogFilter req, CancellationToken ct) =>
        await SendSuccessAsync(await _dispatcher.QueryAsync(new GetAuditLogsQuery(req), ct), ct: ct);
}