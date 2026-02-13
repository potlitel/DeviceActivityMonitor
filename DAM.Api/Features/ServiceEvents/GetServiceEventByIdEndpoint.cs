using DAM.Api.Base;
using DAM.Core.DTOs.Events;
using DAM.Core.Features.Events.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.ServiceEvents
{
    /// <summary>
    /// 📊 Obtiene un evento específico de la "caja negra" del sistema por su ID.
    /// </summary>
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
