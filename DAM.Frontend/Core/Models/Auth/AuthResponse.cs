using System.Text.Json.Serialization;

namespace DAM.Frontend.Core.Models.Auth
{
    //public record AuthResponse(
    //[property: JsonPropertyName("token")] string Token,
    //[property: JsonPropertyName("refreshToken")] string RefreshToken,
    //[property: JsonPropertyName("expiresAt")] DateTime ExpiresAt,
    //[property: JsonPropertyName("requiresTwoFactor")] bool RequiresTwoFactor = false);

    public class AuthResponse
    {
        public string Token { get; set; }
        public string UserEmail { get; set; }
        public DateTime ExpiresAt { get; set; }
        // RefreshToken no viene, así que puede ser opcional
        public string? RefreshToken { get; set; }
    }


}
