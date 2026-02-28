using System.Text.Json.Serialization;

namespace DAM.Frontend.Core.Models.Presence
{
    public record PresenceDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("serialNumber")] string SerialNumber,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("activityId")] int? ActivityId,
    [property: JsonPropertyName("activity")] Activities.ActivityDto? Activity);
}
