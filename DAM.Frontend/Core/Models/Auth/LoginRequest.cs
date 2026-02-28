using System.Text.Json.Serialization;

namespace DAM.Frontend.Core.Models.Auth
{
    public record LoginRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password);
}
