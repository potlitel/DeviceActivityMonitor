using DAM.Api.Base;
using DAM.Core.Common;
using DAM.Core.DTOs.Events;
using DAM.Core.Features.Events.Queries;
using DAM.Infrastructure.CQRS;

public class GetServiceEventsEndpoint(IDispatcher dispatcher)
    : BaseEndpoint<ServiceEventFilter, PaginatedList<ServiceEventDto>>
{
    public override void Configure()
    {
        Get("/system/events");
        Roles("Manager");
        Summary(s => {
            s.Summary = "Consulta la 'Caja Negra' del sistema.";
            s.Description = "Muestra logs detallados del funcionamiento interno del Worker Service y el Watcher.";
        });
    }

    public override async Task HandleAsync(ServiceEventFilter req, CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetServiceEventsQuery(req), ct);
        await SendSuccessAsync(result, "Eventos de sistema recuperados correctamente.", ct);
    }
}