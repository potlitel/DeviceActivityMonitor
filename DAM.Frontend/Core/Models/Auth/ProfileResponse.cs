using System.Text.Json.Serialization;

namespace DAM.Frontend.Core.Models.Auth
{
    public record ProfileResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("isTwoFactorEnabled")] bool IsTwoFactorEnabled,
    [property: JsonPropertyName("preferences")] UserPreferences Preferences);

    public record UserPreferences(
        [property: JsonPropertyName("theme")] string Theme = "Light",
        [property: JsonPropertyName("language")] string Language = "es-ES");
}
