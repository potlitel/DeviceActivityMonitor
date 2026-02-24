using DAM.Api.Base;
using DAM.Core.Features.ServiceHeartBeats;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.Heartbeat
{
    /// <summary>
    /// 🖥️ Registra un latido de salud (heartbeat) desde un servicio worker.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> POST</description></item>
    /// <item><description><b>Ruta:</b> /service/heartbeat</description></item>
    /// <item><description><b>Autenticación:</b> No requerida (Servicio Worker)</description></item>
    /// <item><description><b>Roles permitidos:</b> Anonymous (solo para worker service interno)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>💓 ¿Qué es un heartbeat?</b>
    /// Los servicios worker envían latidos periódicamente (cada 30 segundos) para indicar que están operativos.
    /// Este endpoint recibe esos latidos y actualiza el estado en caché.
    /// </para>
    /// <para>
    /// <b>📊 Datos que se registran:</b>
    /// <list type="bullet">
    /// <item><description><b>MachineName:</b> Identificador único del servicio</description></item>
    /// <item><description><b>Timestamp:</b> Momento exacto del latido</description></item>
    /// <item><description><b>Status:</b> Estado actual del servicio</description></item>
    /// <item><description><b>Version:</b> Versión del software en ejecución</description></item>
    /// <item><description><b>ActiveMonitors:</b> Número de monitores activos</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>⚙️ Mecanismo de detección de fallos:</b>
    /// <list type="number">
    /// <item><description>El servicio envía heartbeat cada 30s</description></item>
    /// <item><description>Se actualiza el timestamp en caché con TTL de 45s</description></item>
    /// <item><description>Si pasan 45s sin heartbeat → servicio considerado OFFLINE</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>🔐 Seguridad:</b>
    /// Aunque es anónimo, el servicio debe identificarse con su MachineName válido.
    /// En producción, considera agregar validación de IP o API Key.
    /// </para>
    /// </remarks>
    /// <response code="200">✅ Heartbeat procesado correctamente</response>
    /// <response code="400">❌ Datos de heartbeat inválidos</response>
    /// <response code="500">❌ Error interno al procesar el heartbeat</response>
    public class PostHeartbeatEndpoint(IDispatcher dispatcher)
        : BaseEndpoint<ServiceHeartbeatCmd, bool>
    {
        public override void Configure()
        {
            Post("/service/heartbeat");
            AllowAnonymous(); // El servicio se identifica mediante el MachineName en el comando

            Description(x => x
                .Accepts<ServiceHeartbeatCmd>("application/json")
                .Produces<bool>(200)
                .ProducesProblem(400)
                .ProducesProblem(500)
                .WithTags("🖥️ Sistema")
                .WithDescription("""
                Recibe y procesa los latidos de salud enviados por los servicios worker.
                
                **📋 Ejemplo de request:**
                ```json
                {
                    "machineName": "SRV-WORKER-01",
                    "timestamp": "2024-01-15T10:30:00Z",
                    "status": "Healthy",
                    "version": "1.2.3.4",
                    "activeMonitors": 3,
                    "additionalInfo": {
                        "cpuUsage": 15.5,
                        "memoryUsage": 1024,
                        "diskSpace": 51200
                    }
                }
                ```
                """));

            Summary(s =>
            {
                s.Summary = "🖥️ [Sistema] Registra heartbeat del servicio worker";
                s.Description = "Endpoint interno para que los workers reporten su estado de salud periódicamente.";
            });
        }

        public override async Task HandleAsync(ServiceHeartbeatCmd req, CancellationToken ct)
        {
            var success = await dispatcher.SendAsync<bool>(req, ct);

            if (!success)
            {
                await SendErrorsAsync(500, ct);
                return;
            }

            await SendSuccessAsync(true, "💓 Latido procesado correctamente.", ct);
        }
    }
}
