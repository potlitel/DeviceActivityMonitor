using DAM.Core.Entities;
using DAM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace DAM.Infrastructure.Storage
{
    public class ApiStorageService : IActivityStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiStorageService> _logger;

        public ApiStorageService(HttpClient httpClient, ILogger<ApiStorageService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            // Configurar base address del HttpClient en Program.cs
        }

        public async Task StoreActivityAsync(DeviceActivity activity)
        {
            // Serializar y enviar a la API
            var response = await _httpClient.PostAsJsonAsync("api/activities", activity);
            response.EnsureSuccessStatusCode();
        }

        public async Task StoreServiceEventAsync(ServiceEvent serviceEvent)
        {
            var response = await _httpClient.PostAsJsonAsync("api/serviceevents", serviceEvent);
            response.EnsureSuccessStatusCode();
        }
    }
}
