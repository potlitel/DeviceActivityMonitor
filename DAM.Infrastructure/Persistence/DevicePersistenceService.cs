using DAM.Core.Constants;
using DAM.Core.DTOs.Heartbeat;
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
                await storageService.BeginTransactionAsync();

                // 1. Guardamos actividad (genera ID al hacer SaveChanges)
                await storageService.StoreActivityAsync(activity);
                await storageService.SaveChangesAsync();

                // 2. Guardamos presencia vinculada
                await storageService.StoreDevicePresenceAsync(new DevicePresence
                {
                    DeviceActivityId = activity.Id,
                    SerialNumber = activity.SerialNumber,
                    Timestamp = DateTime.UtcNow
                });
                await storageService.SaveChangesAsync();

                await storageService.CommitTransactionAsync();
                _logger.LogInformation(Messages.Persistence.PresenceSaved, activity.Id);
            }
            catch (Exception ex)
            {
                await storageService.RollbackTransactionAsync();
                _logger.LogError(ex, Messages.Persistence.PresenceFailed, activity.SerialNumber);
            }
        }

        /// <inheritdoc/>
        public async Task PersistActivityAsync(DeviceActivity activity)
        {
            using var scope = _scopeFactory.CreateScope();
            var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();

            try
            {
                if (activity.Id == 0)
                    await storageService.StoreActivityAsync(activity);
                else
                    await storageService.UpdateActivityAsync(activity);

                await storageService.SaveChangesAsync();
                _logger.LogInformation("Actividad {Id} persistida.", activity.Id);
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
                await storageService.SaveChangesAsync();
                _logger.LogInformation(Messages.Persistence.EventSaved, serviceEvent.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Persistence.EventFailed, serviceEvent.EventType);
            }
        }

        /// <inheritdoc/>
        public async Task PersistInvoiceAsync(DeviceActivity activity, bool uptHorphanActvs)
        {
            if (activity.FilesCopied.Count == 0) return;

            using var scope = _scopeFactory.CreateScope();
            var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();

            try
            {
                var invoice = _invoiceCalculator.CalculateInvoice(activity);
                if (invoice != null)
                {
                    await storageService.BeginTransactionAsync();

                    invoice.DeviceActivityId = activity.Id;
                    await storageService.StoreInvoiceAsync(invoice);
                    if (uptHorphanActvs)
                        await storageService.UpdateActivityAsync(activity); // new line
                    await storageService.SaveChangesAsync();

                    await storageService.CommitTransactionAsync();
                    _logger.LogInformation(Messages.Persistence.InvoiceSaved, invoice.TotalAmount, invoice.SerialNumber);
                }
            }
            catch (Exception ex)
            {
                await storageService.RollbackTransactionAsync();
                _logger.LogError(ex, Messages.Persistence.InvoiceFailed, activity.SerialNumber);
            }
        }

        /// <inheritdoc/>
        public async Task RecoverPendingActivitiesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();


            try
            {
                // 1. Buscar actividades que tengan archivos pero no factura
                var pendingActivities = await uow.Activities.GetActivitiesMissingInvoicesAsync();

                if (pendingActivities.Any())
                {
                    var options = new ParallelOptions { MaxDegreeOfParallelism = 5 };

                    await Parallel.ForEachAsync(pendingActivities, options, async (activity, token) =>
                    {
                        try
                        {
                            _logger.LogWarning("Recuperando actividad huérfana: {Id}", activity.Id);
                            await PersistInvoiceAsync(activity, true);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "No se pudo recuperar la actividad huérfana: {Id}", activity.Id);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task SendHeartbeatAsync(HeartbeatDto heartbeat)
        {
            using var scope = _scopeFactory.CreateScope();
            var storageService = scope.ServiceProvider.GetRequiredService<IActivityStorageService>();

            try
            {
                await storageService.SendHeartbeatAsync(heartbeat);
                _logger.LogInformation(Messages.HeartBeat.HeartBeatSendSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.HeartBeat.HeartBeatSendError);
            }
        }
    }
}
