using System.Security.Claims;
using System.Text.Json;

namespace DAM.Frontend.Infrastructure.Extensions
{
    /// <summary>
    /// 🔐 Parser de JWT Token sin dependencias externas
    /// </summary>
    public static class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs == null) return claims;

            foreach (var (key, value) in keyValuePairs)
            {
                if (value is JsonElement element)
                {
                    claims.AddRange(ParseClaimsFromJsonElement(key, element));
                }
            }

            return claims;
        }

        private static IEnumerable<Claim> ParseClaimsFromJsonElement(string key, JsonElement element)
        {
            var claims = new List<Claim>();

            if (key is "role" or "roles")
            {
                if (element.ValueKind == JsonValueKind.Array)
                {
                    claims.AddRange(element.EnumerateArray()
                        .Select(role => new Claim(ClaimTypes.Role, role.ToString())));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, element.ToString()));
                }
            }
            else
            {
                claims.Add(new Claim(key switch
                {
                    "nameid" or "sub" => ClaimTypes.NameIdentifier,
                    "email" => ClaimTypes.Email,
                    "name" => ClaimTypes.Name,
                    _ => key
                }, element.ToString()));
            }

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            base64 = (base64.Length % 4) switch
            {
                2 => base64 + "==",
                3 => base64 + "=",
                _ => base64
            };

            return Convert.FromBase64String(base64);
        }
    }
}
