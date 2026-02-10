using DAM.Api.Base;
using DAM.Core.DTOs.DeviceActivity;
using DAM.Core.Features.Activities.Queries;
using DAM.Infrastructure.CQRS;
using FastEndpoints;

namespace DAM.Api.Features.DeviceActivity
{
    public class GetActivityByIdEndpoint(IDispatcher d) : BaseEndpoint<GetByIdRequest, DeviceActivityDto>
    {
        public override void Configure() { Get("/activities/{id}"); Roles("Manager"); }
        public override async Task HandleAsync(GetByIdRequest r, CancellationToken ct)
        {
            var res = await d.QueryAsync(new GetActivityByIdQuery(r.Id), ct);
            if (res == null) await SendErrorsAsync(404, ct); else await SendSuccessAsync(res);
        }
    }
}
