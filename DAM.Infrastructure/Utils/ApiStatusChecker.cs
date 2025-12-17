using DAM.Core.Constants;
using DAM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DAM.Infrastructure.Utils
{
    /// <summary>
    /// Implementación concreta de la verificación de estado de la Web API utilizando HttpClient.
    /// </summary>
    public class ApiStatusChecker : IApiStatusChecker
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiStatusChecker> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="ApiStatusChecker"/>.
        /// </summary>
        /// <param name="httpClient">Cliente HTTP inyectado, configurado con la URL base de la API.</param>
        /// <param name="logger">Servicio de logging para registrar fallos.</param>
        public ApiStatusChecker(HttpClient httpClient, ILogger<ApiStatusChecker> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                // Endpoint de Health Check (ej: /health)
                var response = await _httpClient.GetAsync("health");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(Messages.Api.NotAvailable, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Api.UnknownError);
                return false;
            }
        }
    }
}
