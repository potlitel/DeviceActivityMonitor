using FastEndpoints;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DAM.Api.Features.Diagnostics
{
    public class ReadyEndpoint : EndpointWithoutRequest
    {
        private readonly HealthCheckService _healthService;
        public ReadyEndpoint(HealthCheckService healthService) => _healthService = healthService;

        public override void Configure()
        {
            Get("/health/ready");
            AllowAnonymous();
            Description(x => x.WithTags("🔧 Diagnóstico").WithName("HealthReady"));
            Summary(s => s.Summary = "🩺 [Health] Verificar preparación de dependencias");

            Summary(s =>
            {
                s.Summary = "🔧 [Diagnóstico] Verificar estado del servicio";
                s.Description = "Realiza un chequeo de salud (Health Check) básico para validar la conectividad con la API.";
                s.Responses[200] = "La conexión fue exitosa y el servidor está operativo.";
            });

            Description(x => x
                .WithTags("🔧 Diagnóstico")
                .Produces(200)
                .WithDescription("""
                **Uso técnico:**
                - Ideal para scripts de CI/CD post-despliegue.
                - No requiere token de autenticación (público).
                """));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var report = await _healthService.CheckHealthAsync(ct);
            var response = new
            {
                Status = report.Status.ToString(),
                Checks = report.Entries.Select(e => new { Component = e.Key, Status = e.Value.Status.ToString() })
            };
            await Send.OkAsync(response, ct);
        }
    }
}
