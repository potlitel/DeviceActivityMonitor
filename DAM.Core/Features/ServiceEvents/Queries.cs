using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.Events;

namespace DAM.Core.Features.Events.Queries;

/// <summary> Consulta los logs internos del sistema. </summary>
public record GetServiceEventsQuery(ServiceEventFilter Filter) : IQuery<PaginatedList<ServiceEventDto>>;

/// <summary> Detalle técnico de un evento de error o advertencia del servicio. </summary>
public record GetServiceEventByIdQuery(int Id) : IQuery<ServiceEventDto?>;