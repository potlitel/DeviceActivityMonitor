using DAM.Api.Base;
using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.Audit;
using DAM.Infrastructure.CQRS;
using FastEndpoints;

namespace DAM.Api.Features.Audit;

// 1. Definimos la Query (CQRS)
public record GetAuditLogsQuery(AuditLogFilter Filter) : IQuery<PaginatedList<AuditLogResponse>>;

// 2. El Endpoint
public class GetAuditLogsEndpoint : BaseEndpoint<AuditLogFilter, ApiResponse<PaginatedList<AuditLogResponse>>>
{
    private readonly IDispatcher _dispatcher;

    public GetAuditLogsEndpoint(IDispatcher dispatcher) => _dispatcher = dispatcher;

    public override void Configure()
    {
        Get("/audit/logs");
        Roles("Manager"); // Seguridad: Solo Managers
        Summary(s => {
            s.Summary = "Obtiene el historial de auditoría del sistema.";
            s.Description = "Permite filtrar por usuario, fecha y acción realizada.";
        });
    }

    public override async Task HandleAsync(AuditLogFilter req, CancellationToken ct)
    {
        var query = new GetAuditLogsQuery(req);
        var result = await _dispatcher.QueryAsync(query, ct);

        await SendSuccessAsync(ApiResponse<PaginatedList<AuditLogResponse>>.Ok(result), ct: ct);
    }
}