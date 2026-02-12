using DAM.Api.Base;
using DAM.Core.DTOs.Login;
using DAM.Infrastructure.CQRS;
using DAM.Infrastructure.Identity;
using Microsoft.AspNetCore.Components;

//public class LoginRequest { 
//    public string Username { get; set; } = null!; 
//    public string Password { get; set; } = null!; 
//}

//public class LoginResponse
//{
//    public bool RequiresTwoFactor { get; set; }
//    public string? AccessToken { get; set; }
//    public string? RefreshToken { get; set; }
//}

/// <summary>
/// 🔐 Autentica un usuario y genera un token JWT para acceso a la API.
/// </summary>
/// <remarks>
/// <para>
/// <b>🔍 Detalles del endpoint:</b>
/// <list type="bullet">
/// <item><description><b>Método:</b> POST</description></item>
/// <item><description><b>Ruta:</b> /auth/login</description></item>
/// <item><description><b>Autenticación:</b> Anónima (AllowAnonymous)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>🔐 Flujo de autenticación:</b>
/// <list type="number">
/// <item><description>Cliente envía credenciales (email/password)</description></item>
/// <item><description>Servidor valida credenciales contra BD</description></item>
/// <item><description>Si son válidas, genera JWT con claims de usuario y rol</description></item>
/// <item><description>Cliente debe incluir el token en header `Authorization: Bearer {token}`</description></item>
/// </list>
/// </para>
/// <para>
/// <b>⚠️ Seguridad:</b>
/// - Las contraseñas son verificadas con BCrypt (WorkFactor 12)
/// - No se revela qué parte de la credencial falló (email o password)
/// - Los intentos fallidos son registrados para detección de ataques de fuerza bruta
/// - El token expira según configuración (por defecto: 60 minutos)
/// </para>
/// </remarks>
/// <response code="200">✅ Autenticación exitosa - Retorna token JWT</response>
/// <response code="400">❌ Credenciales no enviadas o formato inválido</response>
/// <response code="401">❌ Credenciales inválidas - Mensaje genérico por seguridad</response>
public class LoginEndpoint(IDispatcher dispatcher) : BaseEndpoint<LoginRequest, LoginResponse>
{
    private readonly IIdentityService _identityService;

    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();

        Description(x => x
            .Produces<LoginResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .WithTags("🔐 Autenticación")
            .WithDescription("""
                # 🔐 Autenticación JWT
                
                Este endpoint inicia sesión y retorna un token Bearer JWT.
                
                ## 📌 Ejemplo de uso:
                ```json
                {
                  "email": "admin@dam.com",
                  "password": "Admin123!"
                }
                ```
                
                ## 📤 Respuesta exitosa:
                ```json
                {
                  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                  "refreshToken": "a1b2c3d4e5f6...",
                  "expiresAt": "2026-02-11T16:30:45Z",
                  "requiresTwoFactor": false
                }
                ```
                
                ## 🔑 Uso del token:
                Incluya el token en el header de todas las peticiones posteriores:
                ```
                Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
                ```
                """));

        Summary(s =>
        {
            s.Summary = "🔐 [Auth] Inicia sesión y obtiene token JWT";
            s.Description = "Autentica un usuario con email y contraseña. Retorna token JWT para acceso a recursos protegidos.";
            s.ExampleRequest = new LoginRequest("admin@dam.com", "Admin123!");
            s.ResponseExamples[200] = new LoginResponse(
                Token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                UserEmail: "admin@dam.com",
                ExpiresAt: DateTime.UtcNow.AddHours(1)
            );
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var response = await dispatcher.SendAsync<LoginResponse?>(req, ct);

        if (response == null)
        {
            AddError("🔒 Credenciales inválidas. Por favor, verifique su email y contraseña.");
            await SendErrorsAsync(401, ct);
            return;
        }

        await SendSuccessAsync(response, "🔓 Autenticación exitosa", ct);

        #region ToDelete
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
        #endregion
    }
}