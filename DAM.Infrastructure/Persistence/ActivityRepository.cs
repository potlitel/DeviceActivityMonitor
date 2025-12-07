using DAM.Core.Entities;
using DAM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DAM.Infrastructure.Persistence
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly DeviceActivityDbContext _context;
        private readonly ILogger<ActivityRepository> _logger;

        public ActivityRepository(DeviceActivityDbContext context, ILogger<ActivityRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

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
