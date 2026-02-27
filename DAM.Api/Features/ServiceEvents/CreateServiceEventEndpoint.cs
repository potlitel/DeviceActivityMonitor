using DAM.Api.Base;
using DAM.Core.Entities;
using DAM.Core.Features.ServiceEvents.Commands;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.ServiceEvents
{
    /// <summary>
    /// 📥 Registra un nuevo evento de sistema (Caja Negra).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> POST</description></item>
    /// <item><description><b>Ruta:</b> /serviceevents</description></item>
    /// <item><description><b>Autenticación:</b> No requerida (Acceso Público/Interno)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>⚠️ Nota de uso:</b>
    /// Este endpoint permite el registro de logs desde servicios externos o dispositivos 
    /// que no poseen una sesión de usuario Manager activa.
    /// </para>
    /// </remarks>
    /// <response code="200">✅ Evento registrado exitosamente</response>
    /// <response code="400">❌ Error de validación en el cuerpo de la petición</response>
    public class CreateServiceEventEndpoint(IDispatcher dispatcher)
    : BaseEndpoint<CreateServiceEventCmd, int>
    {
        public override void Configure()
        {
            Post("/system/events/");
            AllowAnonymous();

            Description(x => x
                .Produces<int>(200)
                .ProducesProblem(400)
                .WithTags("📊 Sistema/Eventos"));

            Summary(s =>
            {
                s.Summary = "📊 [Sistema] Registrar evento";
                s.Description = "Crea una nueva entrada en el log de eventos del sistema para auditoría y diagnóstico.";
            });
        }

        public override async Task HandleAsync(CreateServiceEventCmd req, CancellationToken ct)
        {
            var resultId = await dispatcher.SendAsync<int>(req, ct);
            await SendSuccessAsync(resultId, "Evento de sistema registrado.", ct);
        }
    }
}
