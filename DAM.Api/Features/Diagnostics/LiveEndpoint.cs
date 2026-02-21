using FastEndpoints;
using DAM.Api.Base;

namespace DAM.Api.Endpoints.Diagnostics;

public record LiveResponse(string Status, DateTime CheckedAt, string Uptime, string Machine);

public class LiveEndpoint : BaseEndpoint<EmptyRequest, LiveResponse>
{
    public override void Configure()
    {
        Get("/health/live");
        AllowAnonymous();
        Description(x => x.WithTags("🖥️ Sistema"));
        Summary(s => {
            s.Summary = "💓 [Health] Verificar vitalidad (Liveness)";
            s.Description = "Confirma que el proceso de la API está en ejecución.";
        });
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

        var data = new LiveResponse(
            Status: "Healthy",
            CheckedAt: DateTime.UtcNow,
            Uptime: $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s",
            Machine: Environment.MachineName
        );

        await SendSuccessAsync(data, "API latiendo correctamente", ct);
    }
}