using DAM.Api.Base;
using DAM.Infrastructure.Identity;
using FastEndpoints;
using FastEndpoints.Security;

namespace DAM.Api.Features.Identity.Profile;

public class Enable2FARequest { public string Code { get; set; } = null!; }

public class Enable2FAEndpoint : BaseEndpoint<Enable2FARequest, object>
{
    private readonly IIdentityService _identityService;
    // Inyectamos nuestro Repositorio/UoW para persistir el cambio

    public override void Configure()
    {
        Post("/identity/profile/2fa/enable");
        Claims("UserId"); // Solo usuarios autenticados
    }

    public override async Task HandleAsync(Enable2FARequest req, CancellationToken ct)
    {
        //var userId = Guid.Parse(User.ClaimValue("UserId")!);
        //var user = await db.Users.FindAsync(new object[] { userId }, ct);

        //var setup = _identityService.GenerateTwoFactorSetup(user!);

        //// Guardar secreto temporal para validar en el siguiente paso
        //user!.TempTwoFactorSecret = setup.Secret;
        //await db.SaveChangesAsync(ct);

        //await SendSuccessAsync(new
        //{
        //    Secret = setup.Secret,
        //    QrUri = setup.QrCodeUri
        //}, "Escanee el código QR en su app autenticadora.");


        //Usar esta variante de código, es necesario inyectar el repositorio de los users!!!
        //var userId = Guid.Parse(User.ClaimValue("UserId")!);
        //var user = await db.Users.FindAsync(new object[] { userId }, ct);

        //if (user?.TempTwoFactorSecret == null) { await SendErrorsAsync(400, ct); return; }

        //var isValid = _identityService.VerifyTwoFactorCode(user.TempTwoFactorSecret, req.Code);
        //if (!isValid) { await SendErrorsAsync(400, ct); return; }

        //// Activar 2FA y generar códigos de respaldo
        //user.IsTwoFactorEnabled = true;
        //user.TwoFactorSecret = user.TempTwoFactorSecret;
        //user.TempTwoFactorSecret = null;

        //var backups = _identityService.GenerateBackupCodes();
        //user.BackupCodesHash = BCrypt.Net.BCrypt.HashPassword(string.Join(",", backups));

        //await db.SaveChangesAsync(ct);
        //await SendSuccessAsync(backups, "2FA activado. Guarde sus códigos de respaldo.");
    }
}