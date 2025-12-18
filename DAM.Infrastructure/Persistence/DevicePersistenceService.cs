using DAM.Core.Constants;
using DAM.Core.Entities;
using DAM.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DAM.Infrastructure.Persistence
{
    public class DevicePersistenceService : IDevicePersistenceService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IInvoiceCalculator _invoiceCalculator;
        private readonly ILogger<DevicePersistenceService> _logger;

        public DevicePersistenceService(IServiceScopeFactory scopeFactory, ILogger<DevicePersistenceService> logger, 
                                        IInvoiceCalculator invoiceCalculator)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _invoiceCalculator = invoiceCalculator;
        }

        /// <inheritdoc/>
        public async Task PersistPresenceAsync(DeviceActivity activity)
        {
            using var scope = _scopeFactory.CreateScope();
            var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();

            try
            {
                await storageService.StoreActivityAsync(activity);

                await storageService.StoreDevicePresenceAsync(new DevicePresence
                {
                    DeviceActivityId = activity.Id,
                    SerialNumber = activity.SerialNumber,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation(Messages.Persistence.PresenceSaved, activity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Persistence.PresenceFailed, activity.Id);
            }
        }

        /// <inheritdoc/>
        public async Task PersistActivityAsync(DeviceActivity activity)
        {
            // La lógica de creación de scope y acceso a repositorios va AQUÍ.
            using var scope = _scopeFactory.CreateScope();
            // El servicio IActivityStorageService se obtiene y se usa dentro del scope
            var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();

            try
            {
                if (activity.Id == 0)
                    await storageService.StoreActivityAsync(activity);
                else
                    await storageService.UpdateActivityAsync(activity);

                _logger.LogInformation("Actividad {Id} actualizada/guardada.", activity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, Messages.Persistence.ActivityCritical, activity.SerialNumber);
            }
        }

        /// <inheritdoc/>
        public async Task PersistServiceEventAsync(ServiceEvent serviceEvent)
        {
            using var scope = _scopeFactory.CreateScope();
            var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();

            try
            {
                await storageService.StoreServiceEventAsync(serviceEvent);

                _logger.LogInformation(Messages.Persistence.EventSaved, serviceEvent.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Persistence.EventFailed, serviceEvent.EventType);
            }
        }

        /// <inheritdoc/>
        public async Task PersistInvoiceAsync(DeviceActivity activity)
        {
            Invoice invoice = null!;
            if (activity.FilesCopied.Count > 0)
            {
                invoice = _invoiceCalculator.CalculateInvoice(activity)!;
            }

            using var scope = _scopeFactory.CreateScope();
            var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();

            try
            {
                if (invoice != null)
                {
                    invoice.DeviceActivityId = activity.Id;
                    await storageService.StoreInvoiceAsync(invoice);

                    _logger.LogInformation(Messages.Persistence.InvoiceSaved,
                                           invoice.TotalAmount, invoice.SerialNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Persistence.InvoiceFailed, invoice.SerialNumber);
            }
        }
    }
}
