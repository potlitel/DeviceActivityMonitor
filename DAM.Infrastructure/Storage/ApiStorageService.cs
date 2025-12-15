using DAM.Core.Entities;
using DAM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace DAM.Infrastructure.Storage
{
    /// <summary>
    /// Implementación del servicio de almacenamiento que envía los datos de actividad a la Web API remota.
    /// </summary>
    public class ApiStorageService : IActivityStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiStorageService> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="ApiStorageService"/>.
        /// </summary>
        /// <param name="httpClient">Cliente HTTP configurado para la API.</param>
        /// <param name="logger">Servicio de logging.</param>
        public ApiStorageService(HttpClient httpClient, ILogger<ApiStorageService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            // Configurar base address del HttpClient en Program.cs
        }

        /// <inheritdoc/>
        public async Task StoreActivityAsync(DeviceActivity activity)
        {
            // Endpoint POST para guardar actividades
            var response = await _httpClient.PostAsJsonAsync("api/activities", activity);
            // Si la respuesta no es exitosa, se lanzará una excepción (manejada por el llamador)
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc/>
        public async Task StoreDevicePresenceAsync(DevicePresence presence)
        {
            // Endpoint POST para guardar historial de presencia
            var response = await _httpClient.PostAsJsonAsync("api/devicepresence", presence);
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc/>
        public async Task StoreServiceEventAsync(ServiceEvent serviceEvent)
        {
            // Endpoint POST para guardar eventos del servicio
            var response = await _httpClient.PostAsJsonAsync("api/serviceevents", serviceEvent);
            response.EnsureSuccessStatusCode();
        }
    }
}
