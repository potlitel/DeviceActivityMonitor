using FastEndpoints;
using DAM.Api.Base;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DAM.Api.Endpoints.Diagnostics;

public record DependencyDetail(string Component, string Status, string Duration, string Description);
public record ReadyResponse(string OverallStatus, string TotalDuration, DateTime CheckedAt, IEnumerable<DependencyDetail> Dependencies);

public class ReadyEndpoint : BaseEndpoint<EmptyRequest, ReadyResponse>
{
    private readonly HealthCheckService _healthService;

    public ReadyEndpoint(HealthCheckService healthService) => _healthService = healthService;

    public override void Configure()
    {
        Get("/health/ready");
        AllowAnonymous();
        Description(x => x.WithTags("🖥️ Sistema"));
        Summary(s => {
            s.Summary = "🧪 [Health] Verificar preparación (Readiness)";
            s.Description = "Valida el estado de la Base de Datos y Almacenamiento.";
        });
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var report = await _healthService.CheckHealthAsync(ct);

        var data = new ReadyResponse(
            OverallStatus: report.Status.ToString(),
            TotalDuration: $"{report.TotalDuration.TotalMilliseconds:N2}ms",
            CheckedAt: DateTime.UtcNow,
            Dependencies: report.Entries.Select(e => new DependencyDetail(
                Component: e.Key,
                Status: e.Value.Status.ToString(),
                Duration: $"{e.Value.Duration.TotalMilliseconds:N2}ms",
                Description: e.Value.Description ?? "Operativo"
            ))
        );

        if (report.Status == HealthStatus.Healthy)
        {
            await SendSuccessAsync(data, "Todos los sistemas operativos", ct);
        }
        else
        {
            // 🛠️ SOLUCIÓN: Convertir explícitamente a List<string> con .ToList()
            var errors = new List<string> { "Uno o más servicios no están listos o están degradados" };

            var response = DAM.Core.Common.ApiResponse<ReadyResponse>.Failure(errors, "Health Check Failed");

            await Send.ResultAsync(Microsoft.AspNetCore.Http.Results.Json(response, statusCode: 503));
        }
    }
}