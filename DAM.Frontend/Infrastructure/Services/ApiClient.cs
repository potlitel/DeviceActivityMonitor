using DAM.Frontend.Core.Interfaces;
using DAM.Frontend.Core.Models.Activities;
using DAM.Frontend.Core.Models.Audit;
using DAM.Frontend.Core.Models.Auth;
using DAM.Frontend.Core.Models.Common;
using DAM.Frontend.Core.Models.Invoices;
using DAM.Frontend.Core.Models.Presence;
using DAM.Frontend.Core.Models.System;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DAM.Frontend.Infrastructure.Services
{
    /// <summary>
    /// 🌐 Cliente HTTP tipado con manejo de errores y reintentos
    /// </summary>
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IStorageService _storage;
        private readonly ILogger<ApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiClient(
            HttpClient httpClient,
            IStorageService storage,
            ILogger<ApiClient> logger)
        {
            _httpClient = httpClient;
            _storage = storage;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _storage.GetAsync<string>("auth_token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task<T?> GetAsync<T>(string endpoint, object? query = null)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var url = BuildUrl(endpoint, query);
                var response = await _httpClient.GetAsync(url);

                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET {Endpoint} failed", endpoint);
                return default;
            }
        }

        private async Task<T?> PostAsync<T>(string endpoint, object? data = null)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST {Endpoint} failed", endpoint);
                return default;
            }
        }

        //private async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response)
        //{
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        _logger.LogWarning("Request failed: {StatusCode} - {Reason}",
        //            (int)response.StatusCode, response.ReasonPhrase);
        //        return default;
        //    }

        //    return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
        //}
        private async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Request failed: {StatusCode}", (int)response.StatusCode);
                return default;
            }

            // 🛠️ Leemos el envoltorio completo primero
            var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(_jsonOptions);

            if (wrapper == null) return default;

            if (!wrapper.Success)
            {
                _logger.LogWarning("API Error: {Message}", wrapper.Message);
                return default;
            }

            // ✅ Retornamos solo el contenido de "data"
            return wrapper.Data;
        }

        private string BuildUrl(string endpoint, object? query)
        {
            if (query == null) return endpoint;

            var properties = query.GetType().GetProperties()
                .Where(p => p.GetValue(query) != null)
                .Select(p =>
                {
                    var value = p.GetValue(query);
                    var stringValue = value switch
                    {
                        DateTime dt => dt.ToString("O"),
                        _ => value?.ToString()
                    };

                    return $"{p.Name}={Uri.EscapeDataString(stringValue!)}";
                });

            return properties.Any()
                ? $"{endpoint}?{string.Join("&", properties)}"
                : endpoint;
        }

        // 🔐 Autenticación
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
            => await PostAsync<AuthResponse>("auth/login", request);

        public async Task LogoutAsync()
            => await _storage.ClearAsync();

        public async Task<bool> RefreshTokenAsync()
        {
            var refreshToken = await _storage.GetAsync<string>("auth_refresh_token");
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var response = await PostAsync<AuthResponse>("auth/refresh", new { refreshToken });

            if (response != null)
            {
                await _storage.SetAsync("auth_token", response.Token);
                await _storage.SetAsync("auth_refresh_token", response.RefreshToken);
                await _storage.SetAsync("auth_expiry", response.ExpiresAt.ToString("O"));
                return true;
            }

            return false;
        }

        // 📱 Actividades
        public Task<PaginatedList<ActivityDto>> GetActivitiesAsync(ActivityFilter filter)
            => GetAsync<PaginatedList<ActivityDto>>("activities", filter)
               ?? Task.FromResult(new PaginatedList<ActivityDto>());

        public Task<ActivityDto?> GetActivityByIdAsync(int id)
            => GetAsync<ActivityDto>($"activities/{id}");

        // 👤 Presencia
        public Task<PaginatedList<PresenceDto>> GetPresenceAsync(PresenceFilter filter)
            => GetAsync<PaginatedList<PresenceDto>>("presence", filter)
               ?? Task.FromResult(new PaginatedList<PresenceDto>());

        public Task<PresenceDto?> GetPresenceByIdAsync(int id)
            => GetAsync<PresenceDto>($"presence/{id}");

        // 💰 Facturas
        public Task<PaginatedList<InvoiceDto>> GetInvoicesAsync(InvoiceFilter filter)
            => GetAsync<PaginatedList<InvoiceDto>>("invoices", filter)
               ?? Task.FromResult(new PaginatedList<InvoiceDto>());

        public Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
            => GetAsync<InvoiceDto>($"invoices/{id}");

        // 📋 Auditoría
        public Task<PaginatedList<AuditLogDto>> GetAuditLogsAsync(AuditFilter filter)
            => GetAsync<PaginatedList<AuditLogDto>>("audit/logs", filter)
               ?? Task.FromResult(new PaginatedList<AuditLogDto>());

        public Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id)
            => GetAsync<AuditLogDto>($"audit/{id}");

        // 📊 Sistema
        public Task<PaginatedList<ServiceEventDto>> GetServiceEventsAsync(ServiceEventFilter filter)
            => GetAsync<PaginatedList<ServiceEventDto>>("system/events", filter)
               ?? Task.FromResult(new PaginatedList<ServiceEventDto>());

        public Task<ServiceEventDto?> GetServiceEventByIdAsync(int id)
            => GetAsync<ServiceEventDto>($"system/events/{id}");

        // 👤 Perfil
        public Task<ProfileResponse?> GetProfileAsync()
            => GetAsync<ProfileResponse>("identity/profile");

        public async Task<Setup2FAResponse?> Setup2FAAsync()
        {
            return await PostAsync<Setup2FAResponse>("profile/2fa/setup");
        }

    }
}
