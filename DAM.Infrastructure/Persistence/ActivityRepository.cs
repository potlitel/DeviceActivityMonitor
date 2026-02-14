using DAM.Core.Constants;
using DAM.Core.Entities;
using DAM.Core.Enums;
using DAM.Core.Interfaces;
using DAM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAM.Infrastructure.Persistence
{
    /// <summary>
    /// Implementación concreta del repositorio para interactuar con la base de datos (SQLite) mediante EF Core.
    /// </summary>
    /// <remarks>
    /// Se encarga de las operaciones CRUD básicas y maneja las excepciones de persistencia.
    /// </remarks>
    public class ActivityRepository(DeviceActivityDbContext db, ILogger<ActivityRepository> logger)
    : BaseRepository<DeviceActivity>(db), IActivityRepository
    {
        private readonly ILogger<ActivityRepository> _logger = logger;

        /// <inheritdoc/>
        public async Task AddActivityAsync(DeviceActivity activity)
        {
            try
            {
                await _db.DeviceActivities.AddAsync(activity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Repository.SaveActivityError);
                // Loggear fallo, pero el servicio no debe caer por esto.
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateActivityAsync(DeviceActivity activity)
        {
            try
            {
                activity.Status = ActivityStatus.Completed;
                _db.DeviceActivities.Update(activity);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Repository.UpdateActivityError);
                // Loggear fallo, pero el servicio no debe caer por esto.
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task AddDevicePresenceAsync(DevicePresence presence)
        {
            try
            {
                await _db.DevicePresences.AddAsync(presence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Repository.SavePresenceError);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task AddInvoiceAsync(Invoice invoice)
        {
            try
            {
                await _db.Invoices.AddAsync(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Repository.SaveInvoiceError);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task AddServiceEventAsync(ServiceEvent serviceEvent)
        {
            try
            {
                await _db.ServiceEvents.AddAsync(serviceEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Repository.SaveEventError);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeviceActivity>> GetActivitiesMissingInvoicesAsync()
        {
            return await _db.DeviceActivities
                        .Where(a => a.Status == ActivityStatus.Pending)
                        //.Include(a => a.FilesCopied) // Cargamos solo lo necesario para la factura
                        .AsNoTracking()
                        .ToListAsync();
        }
    }
}
