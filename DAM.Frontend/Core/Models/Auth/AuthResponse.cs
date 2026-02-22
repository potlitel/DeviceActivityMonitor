using System.Text.Json.Serialization;

namespace DAM.Frontend.Core.Models.Auth
{
    public record AuthResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt,
    [property: JsonPropertyName("requiresTwoFactor")] bool RequiresTwoFactor = false);
}
