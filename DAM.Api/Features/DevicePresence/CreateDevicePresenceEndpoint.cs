using DAM.Api.Base;
using DAM.Core.Features.DevicePresence.Commands;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.DevicePresence
{
    /// <summary>
    /// 📥 Registra un nuevo evento de presencia de dispositivo.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> POST</description></item>
    /// <item><description><b>Ruta:</b> /devicepresence</description></item>
    /// <item><description><b>Autenticación:</b> No requerida</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>👣 Registro de actividad:</b>
    /// Utilizado por los sensores o dispositivos para notificar su presencia en tiempo real. 
    /// Al ser de alta frecuencia, se permite el acceso anónimo con validación de ID de dispositivo en el comando.
    /// </para>
    /// </remarks>
    /// <response code="200">✅ Presencia registrada correctamente</response>
    /// <response code="400">❌ Datos del sensor inválidos o mal formateados</response>
    public class CreateDevicePresenceEndpoint(IDispatcher dispatcher)
    : BaseEndpoint<CreateDevicePresenceCmd, int>
    {
        public override void Configure()
        {
            Post("/presence");
            AllowAnonymous();

            Description(x => x
                .Produces<int>(200)
                .ProducesProblem(400)
                .WithTags("👣 Presencia"));

            Summary(s =>
            {
                s.Summary = "👣 [Presencia] Notificar presencia";
                s.Description = "Registra una marca de tiempo y ubicación para un dispositivo específico.";
            });
        }

        public override async Task HandleAsync(CreateDevicePresenceCmd req, CancellationToken ct)
        {
            var resultId = await dispatcher.SendAsync<int>(req, ct);
            await SendSuccessAsync(resultId, "Historial de presencia actualizado.", ct);
        }
    }
}
