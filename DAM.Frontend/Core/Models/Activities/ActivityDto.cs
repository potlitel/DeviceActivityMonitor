using DAM.Frontend.Core.Models.Common;
using System.Text.Json.Serialization;

namespace DAM.Frontend.Core.Models.Activities
{
    public record ActivityDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("serialNumber")] string SerialNumber,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("totalCapacityMB")] long TotalCapacityMB,
    [property: JsonPropertyName("insertedAt")] DateTime? InsertedAt,
    [property: JsonPropertyName("extractedAt")] DateTime? ExtractedAt,
    [property: JsonPropertyName("initialAvailableMB")] long InitialAvailableMB,
    [property: JsonPropertyName("finalAvailableMB")] long FinalAvailableMB)
    {
        public double GbProcessed => (InitialAvailableMB - FinalAvailableMB) / 1024.0;
        public TimeSpan? Duration => ExtractedAt - InsertedAt;
        public bool IsActive => ExtractedAt == null;
    }
}
