using DAM.Api.Base;
using DAM.Core.Common;
using DAM.Core.DTOs.Common;
using DAM.Core.DTOs.DevicePresence;
using DAM.Core.Features.Presence.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.DevicePresence
{
    public class GetPresencesEndpoint(IDispatcher d) : BaseEndpoint<PresenceFilter, PaginatedList<DevicePresenceDto>>
    {
        public override void Configure() { Get("/presence"); Roles("Manager"); }
        public override async Task HandleAsync(PresenceFilter r, CancellationToken ct)
            => await SendSuccessAsync(await d.QueryAsync(new GetPresencesQuery(r), ct), ct: ct);
    }
}
