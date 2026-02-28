using DAM.Api.Base;
using DAM.Core.DTOs.Events;
using DAM.Core.Features.Events.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.ServiceEvents
{
    /// <summary>
    /// 📊 Obtiene un evento específico de la "caja negra" del sistema por su ID.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> GET</description></item>
    /// <item><description><b>Ruta:</b> /system/events/{id}</description></item>
    /// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
    /// <item><description><b>Roles permitidos:</b> Manager</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>🛡️ Auditoría:</b>
    /// Estos eventos registran cambios críticos en el estado del servicio y errores internos. 
    /// Es fundamental para el diagnóstico técnico post-mortem.
    /// </para>
    /// </remarks>
    /// <response code="200">✅ Evento de sistema recuperado</response>
    /// <response code="404">❌ No se encontró el log técnico con ese identificador</response>
    public class GetServiceEventByIdEndpoint(IDispatcher d) : BaseEndpoint<GetByIdIntRequest, ServiceEventDto>
    {
        public override void Configure() {

            Get("/system/events/{id}");
            Roles("Manager");

            Description(x => x
                .Produces<ServiceEventDto>(200)
                .ProducesProblem(401)
                .ProducesProblem(403)
                .ProducesProblem(404)
                .WithTags("📊 Sistema/Eventos"));

            Summary(s =>
            {
                s.Summary = "📊 [Sistema] Detalle de evento de auditoría";
                s.Description = "Obtiene los datos técnicos crudos de un evento registrado por el núcleo del sistema.";
                s.ExampleRequest = new GetByIdIntRequest(999);
            });

        }
        public override async Task HandleAsync(GetByIdIntRequest r, CancellationToken ct)
        {
            var res = await d.QueryAsync(new GetServiceEventByIdQuery(r.Id), ct);

            if (res == null)
            {
                AddError($"❌ No se encontró evento de sistema con ID: {r.Id}");
                await SendErrorsAsync(404, ct);
                return;
            }

            await SendSuccessAsync(res, "✅ Evento de sistema recuperado correctamente", ct);
        }
    }
}
