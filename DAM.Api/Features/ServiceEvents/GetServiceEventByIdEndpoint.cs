using DAM.Api.Base;
using DAM.Core.DTOs.Events;
using DAM.Core.Features.Events.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.ServiceEvents
{
    public class GetServiceEventByIdEndpoint(IDispatcher d) : BaseEndpoint<GetByIdIntRequest, ServiceEventDto>
    {
        public override void Configure() { Get("/system/events/{id}"); Roles("Manager"); }
        public override async Task HandleAsync(GetByIdIntRequest r, CancellationToken ct)
        {
            var res = await d.QueryAsync(new GetServiceEventByIdQuery(r.Id), ct);
            if (res == null) await SendErrorsAsync(404, ct); else await SendSuccessAsync(res, ct: ct);
        }
    }
}
