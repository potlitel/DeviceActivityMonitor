using DAM.Frontend.Core.Interfaces;
using DAM.Frontend.Core.Models.Auth;
using DAM.Frontend.Infrastructure.Authentication;
using Microsoft.AspNetCore.Components.Authorization;

namespace DAM.Frontend.Infrastructure.Services
{
    /// <summary>
    /// 🔐 Servicio de autenticación con JWT
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IApiClient _apiClient;
        private readonly IStorageService _storage;
        private readonly AuthenticationStateProvider _authProvider;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IApiClient apiClient,
            IStorageService storage,
            AuthenticationStateProvider authProvider,
            ILogger<AuthService> logger)
        {
            _apiClient = apiClient;
            _storage = storage;
            _authProvider = authProvider;
            _logger = logger;
        }

        //public async Task<bool> LoginAsync(string email, string password)
        //{
        //    try
        //    {
        //        var request = new LoginRequest(email, password);
        //        var response = await _apiClient.LoginAsync(request);

        //        //if (response == null) return false;
        //        if (response == null)
        //        {
        //            // 🚩 Agrega este log para saber si el API respondió pero el objeto vino vacío
        //            _logger.LogWarning("El API respondió pero el objeto AuthResponse es NULL.");
        //            return false;
        //        }

        //        await _storage.SetAsync("auth_token", response.Token);
        //        await _storage.SetAsync("auth_refresh_token", response.RefreshToken);
        //        await _storage.SetAsync("auth_expiry", response.ExpiresAt.ToString("O"));

        //        if (_authProvider is CustomAuthProvider customProvider)
        //        {
        //            await customProvider.LoginAsync(response.Token);
        //        }

        //        _logger.LogInformation("User logged in successfully: {Email}", email);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Login failed for {Email}", email);
        //        return false;
        //    }
        //}

        //        public async Task<bool> LoginAsync(string email, string password)
        //{
        //    try 
        //    {
        //        var request = new LoginRequest(email, password);
        //        // Cambiamos el tipo esperado al envoltorio
        //        var wrapper = await _apiClient.PostAsync<ApiResponse<AuthResponse>>("auth/login", request);

        //        if (wrapper == null || !wrapper.Success || wrapper.Data == null) return false;

        //        var response = wrapper.Data; // 🔓 Aquí es donde están los tokens reales

        //        await _storage.SetAsync("auth_token", response.Token);
        //        // Manejo seguro si no hay RefreshToken
        //        if (!string.IsNullOrEmpty(response.RefreshToken))
        //            await _storage.SetAsync("auth_refresh_token", response.RefreshToken);

        //        await _storage.SetAsync("auth_expiry", response.ExpiresAt.ToString("O"));

        //        if (_authProvider is CustomAuthProvider customProvider)
        //        {
        //            await customProvider.LoginAsync(response.Token);
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Login failed");
        //        return false;
        //    }
        //}
        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("🔐 Intentando login para: {Email}", email);
                var request = new LoginRequest(email, password);

                var response = await _apiClient.LoginAsync(request);

                if (response == null)
                {
                    _logger.LogWarning("⚠️ Respuesta nula de la API");
                    return false;
                }

                _logger.LogInformation("✅ Token recibido: {TokenLength} caracteres", response.Token.Length);
                _logger.LogDebug("Token: {Token}", response.Token);
                _logger.LogDebug("Expira: {Expires}", response.ExpiresAt);

                await _storage.SetAsync("auth_token", response.Token);

                if (!string.IsNullOrEmpty(response.RefreshToken))
                    await _storage.SetAsync("auth_refresh_token", response.RefreshToken);

                await _storage.SetAsync("auth_expiry", response.ExpiresAt.ToString("O"));

                // 🚨 VERIFICAR QUE SE PUEDE LEER INMEDIATAMENTE
                var savedToken = await _storage.GetAsync<string>("auth_token");
                _logger.LogInformation("📦 Token recuperado después de guardar: {Result}",
                    savedToken != null ? "✅ OK" : "❌ NULL");

                if (savedToken == null)
                {
                    _logger.LogError("❌ El token NO se guardó correctamente en storage");
                }

                if (_authProvider is CustomAuthProvider customProvider)
                {
                    await customProvider.LoginAsync(response.Token);
                }

                _logger.LogInformation("User logged in successfully: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", email);
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await _storage.ClearAsync();

            if (_authProvider is CustomAuthProvider customProvider)
            {
                await customProvider.LogoutAsync();
            }

            _logger.LogInformation("User logged out");
        }

        public async Task<bool> RefreshTokenAsync()
        {
            var result = await _apiClient.RefreshTokenAsync();
            if (result)
            {
                _logger.LogInformation("Token refreshed successfully");
            }
            return result;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _storage.GetAsync<string>("auth_token");
            if (string.IsNullOrEmpty(token)) return false;

            var expiry = await _storage.GetAsync<string>("auth_expiry");
            if (!string.IsNullOrEmpty(expiry) && DateTime.TryParse(expiry, out var expiryDate))
            {
                if (expiryDate < DateTime.UtcNow)
                {
                    await LogoutAsync();
                    return false;
                }
            }

            return true;
        }

        public async Task<string?> GetTokenAsync()
            => await _storage.GetAsync<string>("auth_token");

        public async Task<ProfileResponse?> GetCurrentUserAsync()
        {
            var profile = await _storage.GetAsync<ProfileResponse>("user_profile");

            if (profile == null)
            {
                profile = await _apiClient.GetProfileAsync();
                if (profile != null)
                {
                    await _storage.SetAsync("user_profile", profile);
                }
            }

            return profile;
        }

        public async Task<Setup2FAResponse?> Setup2FAAsync()
        {
            return await _apiClient.Setup2FAAsync();
        }
    }
}
