using DAM.Core.Entities;
using DAM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DAM.Infrastructure.Storage
{
    // Este es el servicio que inyectaremos en el Worker.
    public class ResilientStorageService : IActivityStorageService
    {
        private readonly IApiStatusChecker _apiChecker;
        private readonly LocalDbStorageService _localService;
        private readonly ApiStorageService _apiService;
        private readonly ILogger<ResilientStorageService> _logger;

        // Polling para re-chequear la API
        private bool _isApiAvailable = false;
        private DateTime _lastCheck = DateTime.MinValue;
        private readonly TimeSpan _recheckInterval = TimeSpan.FromMinutes(1);

        // Inyección de servicios concretos y el checker (Strategy Selector)
        public ResilientStorageService(
            IApiStatusChecker apiChecker,
            LocalDbStorageService localService,
            ApiStorageService apiService,
            ILogger<ResilientStorageService> logger)
        {
            _apiChecker = apiChecker;
            _localService = localService;
            _apiService = apiService;
            _logger = logger;
        }

        private async Task<IActivityStorageService> GetCurrentStorageStrategy()
        {
            // Usamos un Circuit Breaker simple: No re-chequeamos demasiado a menudo.
            if (DateTime.Now - _lastCheck > _recheckInterval || _lastCheck == DateTime.MinValue)
            {
                _isApiAvailable = await _apiChecker.IsApiAvailableAsync();
                _lastCheck = DateTime.Now;
            }

            if (_isApiAvailable)
            {
                _logger.LogInformation("Usando estrategia: Web API.");
                return _apiService;
            }
            else
            {
                _logger.LogWarning("Usando estrategia: SQLite Local. Web API no disponible.");
                // TODO: Aquí se añadiría la lógica de reintento de envío (Saga/Outbox Pattern)
                return _localService;
            }
        }

        public async Task StoreActivityAsync(DeviceActivity activity)
        {
            var strategy = await GetCurrentStorageStrategy();
            await strategy.StoreActivityAsync(activity);
        }

        public async Task StoreServiceEventAsync(ServiceEvent serviceEvent)
        {
            var strategy = await GetCurrentStorageStrategy();
            await strategy.StoreServiceEventAsync(serviceEvent);
        }
    }
}
