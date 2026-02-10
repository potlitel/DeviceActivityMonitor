using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.DeviceActivity;

namespace DAM.Core.Features.Activities.Queries;

/// <summary> Consulta para obtener actividades filtradas y paginadas. </summary>
public record GetActivitiesQuery(ActivityFilter Filter) : IQuery<PaginatedList<DeviceActivityDto>>;

/// <summary> Consulta para obtener el detalle de una actividad por su ID. </summary>
public record GetActivityByIdQuery(Guid Id) : IQuery<DeviceActivityDto?>;