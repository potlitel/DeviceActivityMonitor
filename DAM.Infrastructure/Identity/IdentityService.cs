using DAM.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OtpNet;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DAM.Infrastructure.Identity;

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

    /// <summary> Genera 8 códigos alfanuméricos únicos para recuperación de cuenta. </summary>
    /// <remarks>
    /// Genera códigos de un solo uso con alta entropía mediante <see cref="RandomNumberGenerator"/>.
    /// Estos códigos deben almacenarse con hashing en la base de datos para máxima seguridad.
    /// </remarks>
    List<string> GenerateBackupCodes();

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

public class IdentityService(IConfiguration config) : IIdentityService
{
    private readonly IConfiguration _config = config;

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

    public List<string> GenerateBackupCodes()
    {
        return Enumerable.Range(0, 8)
            .Select(_ => RandomNumberGenerator.GetHexString(8).ToUpper())
            .ToList();
    }

    public string CreateJwtToken(ApplicationUser user)
    {
        var secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}