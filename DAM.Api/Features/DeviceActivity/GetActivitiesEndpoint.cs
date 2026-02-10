using DAM.Api.Base;
using DAM.Core.Common;
using DAM.Core.DTOs.Common;
using DAM.Core.DTOs.DeviceActivity;
using DAM.Core.Features.Activities.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.DeviceActivity
{
    public class GetActivitiesEndpoint(IDispatcher d) : BaseEndpoint<ActivityFilter, PaginatedList<DeviceActivityDto>>
    {
        public override void Configure() {
            Get("/activities"); Roles("Manager", "Worker"); 
        }
        public override async Task HandleAsync(ActivityFilter r, CancellationToken ct)
            => await SendSuccessAsync(await d.QueryAsync(new GetActivitiesQuery(r), ct), ct: ct);
    }
}
