using DAM.Frontend.Core.Models.Auth;

namespace DAM.Frontend.Core.Interfaces
{
    /// <summary>
    /// 🔐 Contrato para servicios de autenticación
    /// </summary>
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<bool> RefreshTokenAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string?> GetTokenAsync();
        Task<ProfileResponse?> GetCurrentUserAsync();
        Task<Setup2FAResponse?> Setup2FAAsync();
    }
}
