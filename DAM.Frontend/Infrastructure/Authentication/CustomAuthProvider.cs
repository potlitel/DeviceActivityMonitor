using DAM.Frontend.Core.Interfaces;
using DAM.Frontend.Infrastructure.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace DAM.Frontend.Infrastructure.Authentication
{
    public class CustomAuthProvider : AuthenticationStateProvider
    {
        private readonly IStorageService _storage;
        private readonly ILogger<CustomAuthProvider> _logger;
        private readonly AuthenticationState _anonymous;

        public CustomAuthProvider(
            IStorageService storage,
            ILogger<CustomAuthProvider> logger)
        {
            _storage = storage;
            _logger = logger;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _storage.GetAsync<string>("auth_token");

                if (string.IsNullOrEmpty(token))
                    return _anonymous;

                var expiry = await _storage.GetAsync<string>("auth_expiry");

                if (!string.IsNullOrEmpty(expiry) &&
                    DateTime.TryParse(expiry, out var expiryDate) &&
                    expiryDate < DateTime.UtcNow)
                {
                    await LogoutAsync();
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

        public async Task LoginAsync(string token)
        {
            await _storage.SetAsync("auth_token", token);

            var claims = JwtParser.ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state));
        }

        public async Task LogoutAsync()
        {
            await _storage.ClearAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }
    }
}
