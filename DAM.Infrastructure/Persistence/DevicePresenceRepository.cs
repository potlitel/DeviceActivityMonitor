using DAM.Core.Entities;
using DAM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Infrastructure.Persistence
{
    public class DevicePresenceRepository : IDevicePresenceRepository
    {
        private readonly DeviceActivityDbContext _context;
        private readonly ILogger<ActivityRepository> _logger;

        public DevicePresenceRepository(DeviceActivityDbContext context, ILogger<ActivityRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<IEnumerable<DateTime>> GetPresenceHistoryAsync(string serialNumber)
        {
            throw new NotImplementedException();
        }

        public async Task RegisterPresenceAsync(DevicePresence presence)
        {
            try
            {
                // Simple insert: no se necesita verificar duplicados.
                _context.DevicePresences.Add(presence);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar DevicePresence en la BD.");
                throw;
            }
        }
    }
}
