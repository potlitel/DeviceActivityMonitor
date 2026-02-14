namespace DAM.Core.DTOs.Audit;

/// <summary>
/// DTO para la visualización de trazas de auditoría.
/// </summary>
public record AuditLogResponse(
    Guid Id,
    string Username,
    string Action,    // Ejemplo: "LOGIN_SUCCESS", "INVOICE_CALCULATED"
    string Resource,  // Ejemplo: "/api/invoices"
    string HttpMethod,
    string IpAddress,
    DateTime TimestampUtc
);

/// <summary>
/// Filtros dinámicos para el historial de auditoría.
/// </summary>
public class AuditLogFilter
{
    public string? Username { get; set; }
    public string? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}