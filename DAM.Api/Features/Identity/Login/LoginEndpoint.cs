using DAM.Api.Base;
using DAM.Infrastructure.Identity;

public class LoginRequest { 
    public string Username { get; set; } = null!; 
    public string Password { get; set; } = null!; 
}

public class LoginResponse
{
    public bool RequiresTwoFactor { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}

public class LoginEndpoint : BaseEndpoint<LoginRequest, LoginResponse>
{
    private readonly IIdentityService _identityService;

    public override void Configure()
    {
        Post("/identity/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        // 1. Validar credenciales (Username/Password)
        // 2. Verificar si tiene 2FA activo
        //bool has2FA = true; // Simulación: Obtener de BD

        //if (has2FA)
        //{
        //    await SendSuccessAsync(new LoginResponse { RequiresTwoFactor = true }, "Credenciales válidas. Ingrese código 2FA.");
        //    return;
        //}

        //// 3. Si no tiene 2FA, generar tokens inmediatamente
        //await SendSuccessAsync(new LoginResponse
        //{
        //    RequiresTwoFactor = false,
        //    AccessToken = "JWT_GENERADO_AQUI",
        //    RefreshToken = "REFRESH_TOKEN_AQUI"
        //});

        //var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username, ct);

        //// 1. Validar Password (BCrypt)
        //if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        //{
        //    await SendErrorsAsync(401, ct); // Credenciales inválidas
        //    return;
        //}

        //// 2. Verificar 2FA
        //if (user.IsTwoFactorEnabled)
        //{
        //    await SendSuccessAsync(new LoginResponse { RequiresTwoFactor = true }, "2FA Requerido.");
        //    return;
        //}

        //// 3. Generar Tokens (JWT)
        //var token = _identityService.CreateJwtToken(user);
        //await SendSuccessAsync(new LoginResponse { RequiresTwoFactor = false, AccessToken = token }, "Login exitoso.");
    }
}