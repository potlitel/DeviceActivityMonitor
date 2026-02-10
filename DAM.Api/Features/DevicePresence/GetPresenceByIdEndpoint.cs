using DAM.Api.Base;
using DAM.Core.DTOs.DevicePresence;
using DAM.Core.Features.Presence.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.DevicePresence
{
    public class GetPresenceByIdEndpoint(IDispatcher d) : BaseEndpoint<GetByIdRequest, DevicePresenceDto>
    {
        public override void Configure() { Get("/presence/{id}"); Roles("Manager"); }
        public override async Task HandleAsync(GetByIdRequest r, CancellationToken ct)
        {
            var res = await d.QueryAsync(new GetPresenceByIdQuery(r.Id), ct);
            if (res == null) await SendErrorsAsync(404, ct); else await SendSuccessAsync(res, ct: ct);
        }
    }
}
