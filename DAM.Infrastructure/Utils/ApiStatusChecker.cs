using DAM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Infrastructure.Utils
{
    // Usa HttpClient para enviar un ping a un endpoint de "salud" de la API.
    public class ApiStatusChecker : IApiStatusChecker
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiStatusChecker> _logger;

        public ApiStatusChecker(HttpClient httpClient, ILogger<ApiStatusChecker> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

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
                _logger.LogWarning("Web API no disponible. Falló la conexión: {Message}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error desconocido al verificar la API.");
                return false;
            }
        }
    }
}
