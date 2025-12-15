using DAM.Core.Entities;
using DAM.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Infrastructure.Persistence
{
    public class DevicePersistenceService : IDevicePersistenceService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DevicePersistenceService> _logger;

        public DevicePersistenceService(IServiceScopeFactory scopeFactory, ILogger<DevicePersistenceService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

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
                    _logger.LogInformation("Conexión registrada en BD para {SN}.", serialNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FALLO: No se pudo registrar la presencia del dispositivo {SN} al conectar.", serialNumber);
                }
            }
        }

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

                    _logger.LogInformation("Actividad del dispositivo {SN} persistida exitosamente.", activity.SerialNumber);
                }
                catch (Exception ex)
                {
                    // El log crítico es importante mantenerlo.
                    _logger.LogCritical(ex, "FALLO CRÍTICO: No se pudo persistir la actividad del dispositivo {SN}.", activity.SerialNumber);
                }
            } // El ámbito se desecha aquí.
        }
    }
}
