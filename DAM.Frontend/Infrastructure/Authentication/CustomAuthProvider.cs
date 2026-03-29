using DAM.Frontend.Core.Interfaces;
using DAM.Frontend.Infrastructure.Extensions;
using DAM.Frontend.Infrastructure.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace DAM.Frontend.Infrastructure.Authentication
{
    public class CustomAuthProvider : AuthenticationStateProvider
    {
        private readonly IStorageService _storage;
        private readonly IInMemoryTokenStorage _memoryStorage;
        private readonly ILogger<CustomAuthProvider> _logger;
        private readonly AuthenticationState _anonymous;

        public CustomAuthProvider(
            IStorageService storage,
            IInMemoryTokenStorage memoryStorage,
            ILogger<CustomAuthProvider> logger)
        {
            _storage = storage;
            _memoryStorage = memoryStorage;
            _logger = logger;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Intentar obtener del storage primero, luego de memoria
                var token = await _storage.GetAsync<string>("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    token = _memoryStorage.GetToken();
                }

                if (string.IsNullOrEmpty(token))
                    return _anonymous;

                var expiry = await _storage.GetAsync<string>("auth_expiry");
                if (string.IsNullOrEmpty(expiry))
                {
                    expiry = _memoryStorage.GetExpiry();
                }

                if (!string.IsNullOrEmpty(expiry) &&
                    DateTime.TryParse(expiry, out var expiryDate) &&
                    expiryDate < DateTime.UtcNow)
                {
                    Logout();
                    return _anonymous;
                }

                var claims = JwtParser.ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authentication state");
                return _anonymous;
            }
        }

        public void Login(string token)
        {
            _memoryStorage.SetToken(token);

            var claims = JwtParser.ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state));
        }

        public void Logout()
        {
            _memoryStorage.Clear();
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }
    }
}
