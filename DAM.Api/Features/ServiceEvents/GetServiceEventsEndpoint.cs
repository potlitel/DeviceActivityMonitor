//using DAM.Api.Base;
//using DAM.Core.Common;
//using DAM.Core.DTOs.Common;
//using DAM.Core.DTOs.Events;
//using DAM.Core.Features.Events.Queries;
//using DAM.Infrastructure.CQRS;

//namespace DAM.Api.Features.ServiceEvents
//{
//    public class GetServiceEventsEndpoint(IDispatcher d) : BaseEndpoint<ServiceEventFilter, PaginatedList<ServiceEventDto>>
//    {
//        public override void Configure()
//        {
//            Get("/system/events");
//            Roles("Manager");
//        }
//        public override async Task HandleAsync(ServiceEventFilter r, CancellationToken ct)
//            => await SendSuccessAsync(await d.QueryAsync(new GetServiceEventsQuery(r), ct));
//    }
//}
