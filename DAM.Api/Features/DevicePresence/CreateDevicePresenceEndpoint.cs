using DAM.Api.Base;
using DAM.Core.Features.DevicePresence.Commands;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.DevicePresence
{
    public class CreateDevicePresenceEndpoint(IDispatcher dispatcher)
    : BaseEndpoint<CreateDevicePresenceCmd, int>
    {
        public override void Configure()
        {
            Post("/devicepresence");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateDevicePresenceCmd req, CancellationToken ct)
        {
            var resultId = await dispatcher.SendAsync<int>(req, ct);
            await SendSuccessAsync(resultId, "Historial de presencia actualizado.", ct);
        }
    }
}
