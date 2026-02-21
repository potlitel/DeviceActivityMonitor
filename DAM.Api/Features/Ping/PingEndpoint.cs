using FastEndpoints;

namespace DAM.Api.Endpoints.Diagnostics;

/// <summary>
/// 🔧 Verifica la disponibilidad y el estado operativo de la API.
/// </summary>
/// <remarks>
/// <para>
/// <b>🔍 Propósito:</b>
/// Este endpoint es utilizado por balanceadores de carga, servicios de monitoreo (como UptimeRobot o Prometheus) 
/// y desarrolladores para confirmar que el servicio está arriba y conocer el contexto de ejecución.
/// </para>
/// <para>
/// <b>📊 Información devuelta:</b>
/// <list type="bullet">
/// <item><description><b>Message:</b> Confirmación textual del estado.</description></item>
/// <item><description><b>Timestamp:</b> Hora exacta del servidor en formato UTC.</description></item>
/// <item><description><b>Environment:</b> Entorno de ejecución (Development, Staging, Production).</description></item>
/// <item><description><b>Machine:</b> Nombre del nodo/servidor que procesa la petición.</description></item>
/// </list>
/// </para>
/// </remarks>
/// <response code="200">✅ La API está respondiendo correctamente.</response>
/// <response code="503">❌ El servicio no está disponible o está en mantenimiento.</response>
public class PingEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/ping");
        AllowAnonymous();

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
        //Get("/ping");
        //AllowAnonymous();

        //// Opción A: Usar AllowAnonymous y Tags directamente
        //Summary(s => {
        //    s.Summary = "🔧 [Diagnóstico] Verificar estado del servicio";
        //    s.Description = "Realiza un chequeo de salud (Health Check) básico...";
        //    s.Responses[200] = "La conexión fue exitosa y el servidor está operativo.";
        //});

        //// Usa Options para asegurar que el Tag de Swagger se aplique correctamente
        //Options(x => x.WithTags("Diagnóstico"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(new
        {
            message = "🏓 Pong! API funcionando correctamente",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            machine = Environment.MachineName
        }, ct);
    }
}