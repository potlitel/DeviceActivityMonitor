using DAM.Api.Base;
using DAM.Core.Entities;
using DAM.Core.Features.ServiceEvents.Commands;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.ServiceEvents
{
    public class CreateServiceEventEndpoint(IDispatcher dispatcher)
    : BaseEndpoint<CreateServiceEventCmd, int>
    {
        public override void Configure()
        {
            Post("/serviceevents");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateServiceEventCmd req, CancellationToken ct)
        {
            var resultId = await dispatcher.SendAsync<int>(req, ct);
            await SendSuccessAsync(resultId, "Evento de sistema registrado.", ct);
        }
    }
}
