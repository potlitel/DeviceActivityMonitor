using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.DevicePresence;

namespace DAM.Core.Features.Presence.Queries;

/// <summary> Consulta el historial de conexiones físicas. </summary>
public record GetPresencesQuery(PresenceFilter Filter) : IQuery<PaginatedList<DevicePresenceDto>>;

/// <summary> Detalle de un evento de presencia específico. </summary>
public record GetPresenceByIdQuery(Int32 Id) : IQuery<DevicePresenceDto?>;