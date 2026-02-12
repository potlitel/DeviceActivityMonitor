using DAM.Api.Base;
using DAM.Infrastructure.Identity;
using FastEndpoints;
using FastEndpoints.Security;
using System.Security.Claims;

namespace DAM.Api.Features.Users.Profile;

public record Setup2FAResponse(string Secret, string QrCodeUri, List<string> BackupCodes);

/// <summary>
/// 🔐 Inicia la configuración de autenticación de dos factores (2FA).
/// </summary>
/// <remarks>
/// <para>
/// <b>🔍 Detalles del endpoint:</b>
/// <list type="bullet">
/// <item><description><b>Método:</b> POST</description></item>
/// <item><description><b>Ruta:</b> /profile/2fa/setup</description></item>
/// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
/// <item><description><b>Claims requeridos:</b> UserId</description></item>
/// </list>
/// </para>
/// <para>
/// <b>📱 ¿Cómo funciona 2FA?</b>
/// <list type="number">
/// <item><description>Este endpoint genera un secreto único y un código QR</description></item>
/// <item><description>Usuario escanea el QR con Google Authenticator/Microsoft Authenticator</description></item>
/// <item><description>Usuario confirma con el código de 6 dígitos generado por la app</description></item>
/// <item><description>El secreto se guarda permanentemente y se activa 2FA</description></item>
/// </list>
/// </para>
/// <para>
/// <b>⚠️ IMPORTANTE:</b> Guarde los códigos de respaldo en un lugar seguro.
/// Estos códigos son la ÚNICA forma de recuperar la cuenta si pierde el acceso a la app autenticadora.
/// </para>
/// </remarks>
public class Setup2FAEndpoint(IIdentityService identityService)
    : BaseEndpoint<EmptyRequest, Setup2FAResponse>
{
    public override void Configure()
    {
        Post("/profile/2fa/setup");
        Claims("UserId");

        Description(x => x
            .Produces<Setup2FAResponse>(200)
            .ProducesProblem(401)
            .WithTags("🔐 2FA"));

        Summary(s =>
        {
            s.Summary = "🔐 [2FA] Inicia configuración de autenticación de dos factores";
            s.Description = """
                Genera secreto, código QR y códigos de respaldo para configurar 2FA.
                
                **📱 Apps compatibles:**
                - Google Authenticator
                - Microsoft Authenticator
                - Authy
                - LastPass Authenticator
                """;
        });
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        // 1. Obtener usuario (Simulado aquí por brevedad)
        //var user = new DAM.Core.Entities.ApplicationUser { Email = User.Identity?.Name ?? "user@dam.com" };
        //var userId = User.ClaimValue("UserId");

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            await SendErrorsAsync(401, ct);
            return;
        }

        var user = new DAM.Core.Entities.ApplicationUser { Email = userIdStr };
        var (secret, qrUri) = identityService.GenerateTwoFactorSetup(user);
        var backups = identityService.GenerateBackupCodes();

        await SendSuccessAsync(new Setup2FAResponse(secret, qrUri, backups),
            "🔐 Guarda tus códigos de respaldo y escanea el QR en tu App de autenticación.", ct);
    }
}