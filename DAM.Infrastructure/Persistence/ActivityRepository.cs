using DAM.Core.Constants;
using DAM.Core.Entities;
using DAM.Core.Enums;
using DAM.Core.Interfaces;
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
    public class ActivityRepository : IActivityRepository
    {
        private readonly DeviceActivityDbContext _context;
        private readonly ILogger<ActivityRepository> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="ActivityRepository"/>.
        /// </summary>
        /// <param name="context">El contexto de la base de datos de EF Core.</param>
        /// <param name="logger">El servicio de logging.</param>
        public ActivityRepository(DeviceActivityDbContext context, ILogger<ActivityRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task AddActivityAsync(DeviceActivity activity)
        {
            try
            {
                await _context.DeviceActivities.AddAsync(activity);
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
                _context.DeviceActivities.Update(activity);
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
                await _context.DevicePresences.AddAsync(presence);
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
                await _context.Invoices.AddAsync(invoice);
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
                await _context.ServiceEvents.AddAsync(serviceEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Repository.SaveEventError);
                throw;
            }
        }

        public async Task<IEnumerable<DeviceActivity>> GetActivitiesMissingInvoicesAsync()
        {
            return await _context.DeviceActivities
                        .Where(a => a.Status == ActivityStatus.Pending)
                        .Include(a => a.FilesCopied) // Cargamos solo lo necesario para la factura
                        .AsNoTracking()
                        .ToListAsync();
        }
    }
}
