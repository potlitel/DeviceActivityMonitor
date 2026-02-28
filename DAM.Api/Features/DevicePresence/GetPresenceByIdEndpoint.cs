using DAM.Api.Base;
using DAM.Core.DTOs.DevicePresence;
using DAM.Core.Features.Presence.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.DevicePresence
{
    /// <summary>
    /// 👤 Obtiene un evento de presencia específico por su identificador.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> GET</description></item>
    /// <item><description><b>Ruta:</b> /presence/{id}</description></item>
    /// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
    /// <item><description><b>Roles permitidos:</b> Manager</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>👁️‍🗨️ ¿Qué es un evento de presencia?</b>
    /// Un evento de presencia es una marca temporal que indica que un dispositivo 
    /// fue detectado en el sistema. Múltiples eventos pueden estar asociados a una misma actividad.
    /// </para>
    /// </remarks>
    /// <response code="200">✅ Evento de presencia encontrado y retornado</response>
    /// <response code="401">❌ No autenticado o token inválido</response>
    /// <response code="403">❌ No autorizado - Se requiere rol 'Manager'</response>
    /// <response code="404">❌ No se encontró evento de presencia con el ID especificado</response>
    public class GetPresenceByIdEndpoint(IDispatcher d) : BaseEndpoint<GetByIdIntRequest, DevicePresenceDto>
    {
        public override void Configure() {

            Get("/presence/{id}");
            Roles("Manager");

            Description(x => x
                .Produces<DevicePresenceDto>(200)
                .ProducesProblem(401)
                .ProducesProblem(403)
                .ProducesProblem(404)
                .WithTags("👣 Presencia"));

            Summary(s =>
            {
                s.Summary = "👣 [Presencia] Obtiene evento por ID";
                s.Description = "Recupera la información técnica de una detección de presencia específica.";
                s.ExampleRequest = new GetByIdIntRequest(101);
            });

        }
        public override async Task HandleAsync(GetByIdIntRequest r, CancellationToken ct)
        {
            var res = await d.QueryAsync(new GetPresenceByIdQuery(r.Id), ct);

            if (res == null)
            {
                AddError($"❌ No se encontró evento de presencia con ID: {r.Id}");
                await SendErrorsAsync(404, ct);
                return;
            }

            await SendSuccessAsync(res, "✅ Evento de presencia recuperado correctamente", ct);
        }
    }
}
