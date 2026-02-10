using DAM.Api.Base;
using DAM.Infrastructure.Identity;
using FastEndpoints;
using FastEndpoints.Security;
using System.Security.Claims;

namespace DAM.Api.Features.Users.Profile;

public record Setup2FAResponse(string Secret, string QrCodeUri, List<string> BackupCodes);

public class Setup2FAEndpoint(IIdentityService identityService)
    : BaseEndpoint<EmptyRequest, Setup2FAResponse>
{
    public override void Configure()
    {
        Post("/profile/2fa/setup");
        Claims("UserId");
        Description(x => x.WithTags("Users"));
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
        // 2. Generar secreto y URI para el QR
        var (secret, qrUri) = identityService.GenerateTwoFactorSetup(user);

        // 3. Generar códigos de respaldo
        var backups = identityService.GenerateBackupCodes();

        // NOTA: Aquí deberías guardar el 'secret' temporalmente en la BD 
        // hasta que el usuario confirme que ha escaneado el QR correctamente.

        await SendSuccessAsync(new Setup2FAResponse(secret, qrUri, backups),
            "Guarda tus códigos de respaldo y escanea el QR en tu App de autenticación.", ct);
    }
}