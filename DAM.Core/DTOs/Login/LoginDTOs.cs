using DAM.Core.Abstractions;

namespace DAM.Core.DTOs.Login
{
    /// <summary> Petición de inicio de sesión. </summary>
    public record LoginRequest(string Email, string Password) : ICommand<LoginResponse?>;

    /// <summary> Respuesta tras una autenticación exitosa. </summary>
    /// <param name="Token">JWT generado para el acceso.</param>
    /// <param name="UserEmail">Email del usuario autenticado.</param>
    /// <param name="ExpiresAt">Fecha de expiración del token.</param>
    public record LoginResponse(string Token, string UserEmail, DateTime ExpiresAt);
}
