using DAM.Core.Constants;
using DAM.Core.Entities;
using DAM.Core.Interfaces;
using DAM.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static DAM.Core.Constants.Messages;

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
        //private readonly TimeSpan _recheckInterval = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _recheckInterval;


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
            ILogger<ResilientStorageService> logger,
            IOptions<StorageSettings> settings)
        {
            _apiChecker = apiChecker;
            _localService = localService;
            _apiService = apiService;
            _logger = logger;
            _recheckInterval = TimeSpan.FromMinutes(settings.Value.RecheckIntervalMinutes);
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
                _logger.LogInformation(Messages.Storage.UsingWebApi);
                return _apiService;
            }
            else
            {
                _logger.LogWarning(Messages.Storage.UsingLocalSqlite);
                // TODO: Aquí se añadiría la lógica de reintento de envío (Saga/Outbox Pattern)
                return _localService;
            }
        }

        /// <inheritdoc/>
        public async Task StoreActivityAsync(DeviceActivity activity)
        {
            await (await GetCurrentStorageStrategy()).StoreActivityAsync(activity);
        }

        /// <inheritdoc/>
        public async Task StoreServiceEventAsync(ServiceEvent serviceEvent)
        {
            await (await GetCurrentStorageStrategy()).StoreServiceEventAsync(serviceEvent);
        }

        /// <inheritdoc/>
        public async Task StoreDevicePresenceAsync(DevicePresence presence)
        {
            await (await GetCurrentStorageStrategy()).StoreDevicePresenceAsync(presence);
        }

        /// <inheritdoc/>
        public async Task StoreInvoiceAsync(Core.Entities.Invoice invoice)
        {
            await (await GetCurrentStorageStrategy()).StoreInvoiceAsync(invoice);
        }
        
        /// <inheritdoc/>
        public async Task UpdateActivityAsync(DeviceActivity activity)
        {
            await (await GetCurrentStorageStrategy()).UpdateActivityAsync(activity);
        }

        public async Task BeginTransactionAsync() => await (await GetCurrentStorageStrategy()).BeginTransactionAsync();
        public async Task CommitTransactionAsync() => await(await GetCurrentStorageStrategy()).CommitTransactionAsync();
        public async Task RollbackTransactionAsync() => await (await GetCurrentStorageStrategy()).RollbackTransactionAsync();
        public async Task SaveChangesAsync() => await (await GetCurrentStorageStrategy()).SaveChangesAsync();
    }
}
