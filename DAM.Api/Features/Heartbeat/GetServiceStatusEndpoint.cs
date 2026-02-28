using DAM.Api.Base;
using DAM.Core.Common;
using DAM.Core.Features.Presence.Queries;
using DAM.Core.Features.ServiceHeartBeats;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.Heartbeat
{
    /// <summary>
    /// 🖥️ Obtiene el estado actual de salud de un servicio worker específico.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> GET</description></item>
    /// <item><description><b>Ruta:</b> /service/status/{machineName}</description></item>
    /// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
    /// <item><description><b>Roles permitidos:</b> Manager, Admin</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>💓 ¿Qué información proporciona?</b>
    /// Este endpoint consulta el estado de salud en tiempo real de un servicio worker específico,
    /// identificado por su nombre de máquina. La información incluye:
    /// <list type="bullet">
    /// <item><description><b>Status:</b> Healthy, Degraded, o Unhealthy</description></item>
    /// <item><description><b>LastHeartbeat:</b> Último latido recibido</description></item>
    /// <item><description><b>Uptime:</b> Tiempo de actividad continuo</description></item>
    /// <item><description><b>SummaryStatus:</b> Resumen legible del estado</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>⚠️ Detección de Offline:</b>
    /// Un servicio se considera OFFLINE cuando no ha enviado latidos en los últimos 45 segundos.
    /// En este caso, el endpoint retorna HTTP 404 con un mensaje descriptivo.
    /// </para>
    /// </remarks>
    /// <param name="machineName">Nombre de la máquina donde corre el servicio (ej. "SRV-WORKER-01")</param>
    /// <response code="200">✅ Servicio activo - Retorna estado detallado</response>
    /// <response code="401">❌ No autenticado o token inválido</response>
    /// <response code="403">❌ No autorizado - Se requiere rol Manager/Admin</response>
    /// <response code="404">❌ Servicio offline - No hay latidos en los últimos 45s</response>
    public class GetServiceStatusEndpoint(IDispatcher dispatcher)
        : BaseEndpoint<string, ServiceStatusResponse>
    {
        public override void Configure()
        {
            Get("/service/status/{machineName}");
            Roles("Manager", "Admin");

            Description(x => x
                .Produces<ServiceStatusResponse>(200)
                .ProducesProblem(401)
                .ProducesProblem(403)
                .ProducesProblem(404)
                .WithTags("🖥️ Sistema")
                .WithDescription("""
                Consulta el estado de salud en tiempo real de un servicio worker.
                
                **📋 Ejemplo de respuesta (Healthy):**
                ```json
                {
                    "machineName": "SRV-WORKER-01",
                    "status": "Healthy",
                    "lastHeartbeat": "2024-01-15T10:30:00Z",
                    "uptime": "2d 4h 15m",
                    "summaryStatus": "Operational",
                    "version": "1.2.3.4",
                    "activeMonitors": 3
                }
                ```
                
                **📋 Ejemplo de respuesta (Offline):**
                ```json
                {
                    "status": 404,
                    "message": "OFFLINE",
                    "errors": ["El servicio no ha reportado actividad en los últimos 45 segundos."]
                }
                ```
                """));

            Summary(s =>
            {
                s.Summary = "🖥️ [Sistema] Obtiene estado de salud del servicio worker";
                s.Description = "Consulta el estado actual de un servicio específico. Retorna 404 si está offline.";
                s.ExampleRequest = "SRV-WORKER-01";
            });
        }

        public override async Task HandleAsync(string machineName, CancellationToken ct)
        {
            var query = new GetServiceStatusQuery(machineName);
            var result = await dispatcher.QueryAsync<ServiceStatusResponse>(query!, ct);

            if (result == null)
            {
                // Caso OFFLINE: No hay datos en caché (pasaron más de 45s)
                var response = ApiResponse<ServiceStatusResponse>.Failure(
                    ["El servicio no ha reportado actividad en los últimos 45 segundos."],
                    "OFFLINE"
                );

                await Send.ResultAsync(Results.Json(response, statusCode: 404));
                return;
            }

            await SendSuccessAsync(result, $"Servicio reportado como {result.SummaryStatus}", ct);
        }
    }
}
