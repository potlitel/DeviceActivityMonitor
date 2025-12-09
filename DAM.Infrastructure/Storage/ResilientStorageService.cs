using DAM.Core.Entities;
using DAM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DAM.Infrastructure.Storage
{
    /// <summary>
    /// Servicio de almacenamiento principal que implementa la estrategia de resiliencia (Circuit Breaker).
    /// </summary>
    /// <remarks>
    /// Decide dinámicamente si usar <see cref="ApiStorageService"/> (remoto) o <see cref="LocalDbStorageService"/> (local)
    /// basándose en la disponibilidad de la Web API.
    /// </remarks>
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

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="ResilientStorageService"/>.
        /// </summary>
        /// <param name="apiChecker">Servicio para verificar el estado de la API.</param>
        /// <param name="localService">Servicio de almacenamiento local.</param>
        /// <param name="apiService">Servicio de almacenamiento remoto (API).</param>
        /// <param name="logger">Servicio de logging.</param>
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

        /// <summary>
        /// Determina la estrategia de almacenamiento actual basándose en la disponibilidad de la API.
        /// </summary>
        /// <returns>La implementación de <see cref="IActivityStorageService"/> a utilizar (API o Local).</returns>
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

        /// <inheritdoc/>
        public async Task StoreActivityAsync(DeviceActivity activity)
        {
            var strategy = await GetCurrentStorageStrategy();
            await strategy.StoreActivityAsync(activity);
        }

        /// <inheritdoc/>
        public async Task StoreServiceEventAsync(ServiceEvent serviceEvent)
        {
            var strategy = await GetCurrentStorageStrategy();
            await strategy.StoreServiceEventAsync(serviceEvent);
        }
    }
}
