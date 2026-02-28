using System.Text.Json.Serialization;

namespace DAM.Frontend.Core.Models.Audit
{
    public record AuditLogDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("resource")] string Resource,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("ipAddress")] string IpAddress,
    [property: JsonPropertyName("userAgent")] string UserAgent);
}
