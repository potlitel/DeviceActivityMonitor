using DAM.Api.Base;
using DAM.Infrastructure.CQRS;
using DAM.Core.Features.DeviceActivity.Commands;

namespace DAM.Api.Features.DeviceActivity
{
    /// <summary>
    /// 📱 Registra una nueva actividad de dispositivo de almacenamiento externo.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> POST</description></item>
    /// <item><description><b>Ruta:</b> /activities</description></item>
    /// <item><description><b>Autenticación:</b> No requerida (Servicio Worker)</description></item>
    /// <item><description><b>Roles permitidos:</b> Anonymous (solo para worker service interno)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>💾 ¿Qué registra este endpoint?</b>
    /// Este endpoint es invocado por el servicio worker cuando detecta la inserción de un nuevo dispositivo USB.
    /// Crea un registro de actividad que representa el inicio del ciclo de vida del dispositivo.
    /// </para>
    /// <para>
    /// <b>📊 Datos que se registran:</b>
    /// <list type="bullet">
    /// <item><description><b>SerialNumber:</b> Identificador único del dispositivo (obligatorio)</description></item>
    /// <item><description><b>Model:</b> Modelo del dispositivo (obligatorio)</description></item>
    /// <item><description><b>TotalCapacityMB:</b> Capacidad total en MB (obligatorio)</description></item>
    /// <item><description><b>InsertedAt:</b> Fecha y hora de inserción (automática)</description></item>
    /// <item><description><b>InitialAvailableMB:</b> Espacio disponible al inicio (obligatorio)</description></item>
    /// <item><description><b>Status:</b> Estado inicial de la actividad (Active por defecto)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>⚠️ Importante:</b>
    /// Este endpoint está configurado como anónimo porque es invocado por el servicio worker interno.
    /// En producción, considera agregar una API Key o autenticación de servicio a servicio.
    /// </para>
    /// </remarks>
    /// <response code="200">✅ Actividad creada exitosamente - Retorna el ID generado</response>
    /// <response code="400">❌ Datos de entrada inválidos o incompletos</response>
    /// <response code="500">❌ Error interno al persistir la actividad</response>
    public class CreateActivityEndpoint(IDispatcher dispatcher)
    : BaseEndpoint<CreateActivityCmd, int>
    {
        public override void Configure()
        {
            Post("/activities");
            AllowAnonymous(); // El worker service no tiene autenticación de usuario

            Description(x => x
                .Produces<int>(200)
                .ProducesProblem(400)
                .ProducesProblem(500)
                .WithTags("📱 Actividades")
                .WithDescription("""
                Registra una nueva actividad cuando un dispositivo USB es insertado.
                
                **📋 Ejemplo de request:**
                ```json
                {
                    "serialNumber": "USB-123456789",
                    "model": "Kingston DataTraveler 3.0",
                    "totalCapacityMB": 32768,
                    "insertedAt": "2024-01-15T10:30:00Z",
                    "initialAvailableMB": 15000,
                    "status": 0
                }
                ```
                """));

            Summary(s =>
            {
                s.Summary = "📱 [Actividades] Registra nueva actividad de dispositivo";
                s.Description = "Crea un registro de actividad cuando se detecta la inserción de un USB.";
            });
        }

        public override async Task HandleAsync(CreateActivityCmd req, CancellationToken ct)
        {
            var resultId = await dispatcher.SendAsync<int>(req, ct);
            if (resultId == 0) await SendErrorsAsync(500, ct);

            await SendSuccessAsync(resultId, "Actividad registrada exitosamente.", ct);
        }
    }
}
