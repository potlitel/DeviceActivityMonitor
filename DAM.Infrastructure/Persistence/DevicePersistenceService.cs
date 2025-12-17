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
        public async Task PersistPresenceAsync(string serialNumber)
        {
            // La lógica de creación de scope y acceso a repositorios va AQUÍ.
            using (var scope = _scopeFactory.CreateScope())
            {
                // El servicio IActivityStorageService se obtiene y se usa dentro del scope
                var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();

                try
                {
                    await storageService.StoreDevicePresenceAsync(new DevicePresence
                    {
                        SerialNumber = serialNumber,
                        Timestamp = DateTime.UtcNow
                    });
                    _logger.LogInformation(Messages.Persistence.PresenceSaved, serialNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, Messages.Persistence.PresenceFailed, serialNumber);
                }
            }
        }

        /// <inheritdoc/>
        public async Task PersistActivityAsync(DeviceActivity activity)
        {
            // La lógica de creación de scope y acceso a repositorios va AQUÍ.
            using (var scope = _scopeFactory.CreateScope())
            {
                // El servicio IActivityStorageService se obtiene y se usa dentro del scope
                var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();

                try
                {
                    await storageService.StoreActivityAsync(activity);

                    _logger.LogInformation(Messages.Persistence.ActivitySaved, activity.SerialNumber);
                }
                catch (Exception ex)
                {
                    // El log crítico es importante mantenerlo.
                    _logger.LogCritical(ex, Messages.Persistence.ActivityCritical, activity.SerialNumber);
                }
            } // El ámbito se desecha aquí.
        }

        /// <inheritdoc/>
        public async Task PersistServiceEventAsync(ServiceEvent serviceEvent)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                // Se obtiene el servicio de almacenamiento Scoped dentro del ámbito.
                var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();

                try
                {
                    // Registra el evento de servicio (START/STOP)
                    await storageService.StoreServiceEventAsync(serviceEvent);

                    _logger.LogInformation(Messages.Persistence.EventSaved, serviceEvent.EventType);
                }
                catch (Exception ex)
                {
                    // Se registra un error si la persistencia del evento falla.
                    _logger.LogError(ex, Messages.Persistence.EventFailed, serviceEvent.EventType);
                    // NOTA: No re-lanzamos la excepción ya que el evento debe continuar su flujo.
                }
            }
        }

        /// <inheritdoc/>
        public async Task PersistInvoiceAsync(DeviceActivity activity)
        {
            Invoice invoice = null!;
            if (activity.FilesCopied.Count > 0)
            {
                invoice = _invoiceCalculator.CalculateInvoice(activity);
            }

            // 2. Crear un ámbito (scope) para la transacción de persistencia
            using var scope = _scopeFactory.CreateScope();
            // 3. Obtener el servicio de almacenamiento Scoped (que tiene el DbContext)
            var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();

            try
            {
                // 4. Persistir la factura.
                if (invoice != null)
                {
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
