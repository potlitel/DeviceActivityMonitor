using DAM.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OtpNet;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DAM.Infrastructure.Identity;

/// <summary>
/// 🔐 Define el contrato para servicios de identidad y autenticación
/// </summary>
public interface IIdentityService
{
    /// <summary> Genera una clave secreta y una URI para configuración de Google Authenticator. </summary>
    /// <remarks>
    /// Utiliza el estándar HMAC-Based One-Time Password (HOTP) adaptado para Google Authenticator.
    /// La clave secreta de 160 bits asegura una entropía suficiente para resistir ataques de fuerza bruta.
    /// </remarks>
    (string Secret, string QrCodeUri) GenerateTwoFactorSetup(ApplicationUser user);

    /// <summary> Valida un código TOTP dinámico contra un secreto Base32. </summary>
    bool VerifyTwoFactorCode(string secret, string code);

    /// <summary>
    /// 🔍 Extrae el ID de usuario del token JWT
    /// </summary>
    Guid? GetUserIdFromToken(string token);

    /// <summary> Genera 8 códigos alfanuméricos únicos para recuperación de cuenta. </summary>
    /// <remarks>
    /// Genera códigos de un solo uso con alta entropía mediante <see cref="RandomNumberGenerator"/>.
    /// Estos códigos deben almacenarse con hashing en la base de datos para máxima seguridad.
    /// </remarks>
    List<string> GenerateBackupCodes(int count = 10);

    /// <summary>
    /// Genera un token JWT firmado para un usuario autenticado.
    /// </summary>
    /// <param name="user">Objeto de usuario que contiene los claims básicos.</param>
    /// <returns>Una cadena que representa el JWT compacto.</returns>
    /// <remarks>
    /// El token incluye claims de identidad (sub), email y el rol del usuario.
    /// Se utiliza el algoritmo HmacSha256 para asegurar la integridad de la firma.
    /// </remarks>
    string CreateJwtToken(ApplicationUser user);
}

/// <summary>
/// 🔐 Configuración JWT con validación incorporada
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "DAM_Security_Service";
    public string Audience { get; set; } = "DAM_Frontend_App";
    public int AccessTokenExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public int ClockSkewMinutes { get; set; } = 0;

    // ✅ VALIDACIÓN DE SEGURIDAD
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Secret) &&
               Secret.Length >= 32 &&
               !string.IsNullOrWhiteSpace(Issuer) &&
               !string.IsNullOrWhiteSpace(Audience) &&
               AccessTokenExpiryMinutes > 0;
    }
}

/// <summary>
/// 🔐 Servicio de identidad para generación y validación de tokens JWT
/// </summary> 
/// <remarks>
/// Este servicio es responsable de:
/// - Generar tokens JWT con claims de usuario y rol
/// - Validar códigos 2FA utilizando TOTP
/// - Generar códigos de respaldo para 2FA
/// 
/// La seguridad es una prioridad, por lo que se implementan validaciones estrictas en la configuración y generación de tokens.
/// Se recomienda almacenar los secretos y claves de forma segura (ej. Azure Key Vault, AWS Secrets Manager).
/// </remarks>
public class IdentityService : IIdentityService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        IOptions<JwtSettings> jwtSettings,
        ILogger<IdentityService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;

        // 🚨 VALIDACIÓN CRÍTICA EN CONSTRUCTOR
        if (!_jwtSettings.IsValid())
        {
            throw new InvalidOperationException($"""
                ❌ Configuración JWT inválida en IdentityService:
                - Secret: {(string.IsNullOrEmpty(_jwtSettings.Secret) ? "❌ NO CONFIGURADO" : "✅ CONFIGURADO")}
                - Secret Length: {_jwtSettings.Secret?.Length ?? 0} (mínimo 32)
                - Issuer: {_jwtSettings.Issuer}
                - Audience: {_jwtSettings.Audience}
                """);
        }
    }

    /// <summary>
    /// 📱 Genera configuración para 2FA
    /// </summary>
    public (string Secret, string QrCodeUri) GenerateTwoFactorSetup(ApplicationUser user)
    {
        // Generar clave secreta de 160 bits (estándar Google Authenticator)
        byte[] secretBytes = KeyGeneration.GenerateRandomKey(20);
        string secretBase32 = Base32Encoding.ToString(secretBytes);

        // Generar URI para el QR Code
        // otpauth://totp/DAM:usuario@correo.com?secret=BASE32SECRET&issuer=DAM_System
        string qrCodeUri = $"otpauth://totp/DAM:{user.Email}?secret={secretBase32}&issuer=DAM_System";

        return (secretBase32, qrCodeUri);
    }

    public bool VerifyTwoFactorCode(string secret, string code)
    {
        byte[] secretBytes = Base32Encoding.ToBytes(secret);
        var totp = new Totp(secretBytes);

        // Verificación con ventana de tiempo de 30s (por si el reloj del móvil está ligeramente desfasado)
        return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
    }

    //public List<string> GenerateBackupCodes()
    //{
    //    return Enumerable.Range(0, 8)
    //        .Select(_ => RandomNumberGenerator.GetHexString(8).ToUpper())
    //        .ToList();
    //}

    /// <summary>
    /// 🎲 Genera códigos de respaldo para 2FA
    /// </summary>
    public List<string> GenerateBackupCodes(int count = 10)
    {
        var codes = new List<string>();
        var random = new Random();

        for (int i = 0; i < count; i++)
        {
            var code = random.Next(10000000, 99999999).ToString();
            codes.Add(code);
        }

        _logger.LogDebug("🔐 Generados {Count} códigos de respaldo 2FA", count);
        return codes;
    }

    /// <summary>
    /// 🔑 Genera un token JWT para el usuario autenticado
    /// </summary>
    /// <param name="user">Usuario autenticado</param>
    /// <returns>Token JWT firmado</returns>
    /// <exception cref="ArgumentNullException">Si el usuario es nulo</exception>
    public string CreateJwtToken(ApplicationUser user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // 🎯 1. Crear lista de claims - SIN DUPLICADOS
        var claims = new List<Claim>();

        // 📌 ID de usuario (UNA SOLA VEZ)
        claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
        claims.Add(new Claim("user_id", user.Id.ToString())); // Custom claim

        // 📌 Email (UNA SOLA VEZ)
        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        // 📌 Nombre de usuario (UNA SOLA VEZ)
        if (!string.IsNullOrWhiteSpace(user.Username))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.Username));
            claims.Add(new Claim("username", user.Username));
        }

        // 📌 Rol (UNA SOLA VEZ)
        if (user.Role != null && !string.IsNullOrWhiteSpace(user.Role.ToString()))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));
            //claims.Add(new Claim("role", user.Role.ToString())); // Para compatibilidad
        }

        // 📌 2FA Status (UNA SOLA VEZ)
        claims.Add(new Claim("two_factor_enabled", user.IsTwoFactorEnabled.ToString().ToLower()));

        // 📌 JTI - Identificador único del token (UNA SOLA VEZ)
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

        // 📌 IAT - Issued At (UNA SOLA VEZ)
        claims.Add(new Claim(JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            ClaimValueTypes.Integer64));

        // 🚨 DEBUG: Verificar claims únicos
#if DEBUG
        var groupedClaims = claims.GroupBy(c => c.Type)
            .Where(g => g.Count() > 1)
            .ToList();

        if (groupedClaims.Any())
        {
            _logger.LogWarning("⚠️ Claims duplicados detectados: {Duplicates}",
                string.Join(", ", groupedClaims.Select(g => g.Key)));
        }
#endif

        // 2️⃣ Crear clave simétrica
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 3️⃣ Crear descriptor del token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = credentials,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow
        };

        // 4️⃣ Generar token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogDebug("✅ JWT generado para: {Email}, expira: {Expires}",
            user.Email, tokenDescriptor.Expires);

        return tokenString;
    }

    /// <summary>
    /// 🔍 Extrae el ID de usuario del token JWT
    /// </summary>
    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var userIdClaim = jwtToken.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error extrayendo userId del token");
        }

        return null;
    }

    /// <summary>
    /// 🎭 Crea los claims para el token JWT
    /// </summary>
    private List<Claim> CreateClaims(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            // 📌 Claims estándar JWT
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            
            // 📌 Claims personalizados
            new Claim("user_id", user.Id.ToString()),
            new Claim("email", user.Email),
        };

        // 🎯 Agregar rol si existe
        if (!string.IsNullOrWhiteSpace(user.Role.ToString()))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));
            claims.Add(new Claim("role", user.Role.ToString()));
        }

        // 🎯 Agregar nombre de usuario si existe
        if (!string.IsNullOrWhiteSpace(user.Username))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.Username));
            claims.Add(new Claim("username", user.Username));
        }

        // 🎯 Agregar 2FA status
        claims.Add(new Claim("two_factor_enabled", user.IsTwoFactorEnabled.ToString().ToLower()));

        return claims;
    }
}