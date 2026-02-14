using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.AuditLogs;

namespace DAM.Core.Features.Audit.Queries;

/// <summary> Consulta las trazas de seguridad y acciones de usuario. </summary>
public record GetAuditLogsQuery(AuditFilter Filter) : IQuery<PaginatedList<AuditLogDto>>;

/// <summary> Detalle de una acción de auditoría específica. </summary>
public record GetAuditLogByIdQuery(Guid Id) : IQuery<AuditLogDto?>;