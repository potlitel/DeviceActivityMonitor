using DAM.Core.Entities;
using DAM.Core.Interfaces;
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
                _context.DeviceActivities.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar DeviceActivity en la BD.");
                // Loggear fallo, pero el servicio no debe caer por esto.
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task AddServiceEventAsync(ServiceEvent serviceEvent)
        {
            try
            {
                _context.ServiceEvents.Add(serviceEvent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar ServiceEvent en la BD.");
                throw;
            }
        }
    }
}
