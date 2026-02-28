using DAM.Api.Base;
using DAM.Core.Features.DeviceActivity.Commands;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.DeviceActivity
{
    /// <summary>
    /// 📱 Actualiza el estado de una actividad de dispositivo (generalmente al extraer el USB).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> PUT</description></item>
    /// <item><description><b>Ruta:</b> /activities/{id}</description></item>
    /// <item><description><b>Autenticación:</b> No requerida (Servicio Worker)</description></item>
    /// <item><description><b>Roles permitidos:</b> Anonymous (solo para worker service interno)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>💾 ¿Qué actualiza este endpoint?</b>
    /// Este endpoint es invocado por el servicio worker cuando detecta la extracción de un dispositivo USB.
    /// Actualiza la actividad existente con los datos de finalización:
    /// <list type="bullet">
    /// <item><description><b>ExtractedAt:</b> Fecha y hora de extracción</description></item>
    /// <item><description><b>FinalAvailableMB:</b> Espacio disponible al final</description></item>
    /// <item><description><b>MegabytesCopied/Deleted:</b> Métricas de transferencia</description></item>
    /// <item><description><b>FilesCopied/Deleted:</b> Lista de archivos transferidos</description></item>
    /// <item><description><b>Status:</b> Actualizado a 'Completed'</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>📊 Fórmulas de cálculo:</b>
    /// <code>
    /// MegabytesCopied = InitialAvailableMB - FinalAvailableMB - MegabytesDeleted
    /// TimeInserted = ExtractedAt - InsertedAt
    /// </code>
    /// </para>
    /// <para>
    /// <b>⚠️ Importante:</b>
    /// Una vez completada, la actividad no puede ser modificada nuevamente.
    /// El sistema calculará automáticamente la duración de la sesión.
    /// </para>
    /// </remarks>
    /// <response code="200">✅ Actividad actualizada exitosamente</response>
    /// <response code="400">❌ Datos de entrada inválidos o estado inconsistente</response>
    /// <response code="404">❌ No se encontró actividad con el ID especificado</response>
    /// <response code="500">❌ Error interno al actualizar la actividad</response>
    public class UpdateActivityStatusEndpoint(IDispatcher dispatcher)
    : BaseEndpoint<UpdateActivityCmd, bool>
    {
        public override void Configure()
        {
            Put("/activities/{id}");
            AllowAnonymous(); // El worker service no tiene autenticación de usuario

            Description(x => x
                .Produces<bool>(200)
                .ProducesProblem(400)
                .ProducesProblem(404)
                .ProducesProblem(500)
                .WithTags("📱 Actividades")
                .WithDescription("""
                Actualiza una actividad existente cuando un dispositivo USB es extraído.
                
                **📋 Ejemplo de request:**
                ```json
                {
                    "id": 123,
                    "extractedAt": "2024-01-15T12:30:00Z",
                    "finalAvailableMB": 14000,
                    "megabytesCopied": 1000,
                    "megabytesDeleted": 0,
                    "filesCopied": ["documento.pdf", "foto.jpg"],
                    "filesDeleted": [],
                    "status": 1
                }
                ```
                """));

            Summary(s =>
            {
                s.Summary = "📱 [Actividades] Actualiza estado de actividad (extracción)";
                s.Description = "Completa una actividad cuando se detecta la extracción del dispositivo USB.";
            });
        }

        public override async Task HandleAsync(UpdateActivityCmd req, CancellationToken ct)
        {
            var success = await dispatcher.SendAsync<bool>(req, ct);

            if (!success)
            {
                AddError("No se encontró la actividad para actualizar o el cambio no fue posible.");
                await SendErrorsAsync(404, ct);
                return;
            }

            await SendSuccessAsync(true, "Estado de actividad actualizado a completado.", ct);
        }
    }
}
