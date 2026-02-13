using FastEndpoints;

namespace DAM.Api.Endpoints.Diagnostics;

/// <summary>
/// 🔧 Endpoint de diagnóstico para verificar conectividad
/// </summary>
public class PingEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/ping");
        AllowAnonymous();
        Description(x => x
            .WithTags("🔧 Diagnóstico")
            .Produces(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(new
        {
            message = "🏓 Pong! API funcionando correctamente",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            machine = Environment.MachineName
        }, ct);
    }
}