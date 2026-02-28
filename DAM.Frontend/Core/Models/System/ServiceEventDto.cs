using MudBlazor;
using System.Text.Json.Serialization;

namespace DAM.Frontend.Core.Models.System
{
    public record ServiceEventDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("level")] string Level,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp)
    {
        public string LevelColor => Level switch
        {
            "Error" => "error",
            "Warning" => "warning",
            "Information" => "info",
            _ => "default"
        };

        public string LevelIcon => Level switch
        {
            "Error" => Icons.Material.Filled.Error,
            "Warning" => Icons.Material.Filled.Warning,
            "Information" => Icons.Material.Filled.Info,
            _ => Icons.Material.Filled.Circle
        };
    }
}
