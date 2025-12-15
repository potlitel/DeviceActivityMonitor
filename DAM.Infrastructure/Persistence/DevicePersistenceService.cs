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
                    _logger.LogInformation("Conexión registrada en BD para {SN}.", serialNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FALLO: No se pudo registrar la presencia del dispositivo {SN} al conectar.", serialNumber);
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

                    _logger.LogInformation("Actividad del dispositivo {SN} persistida exitosamente.", activity.SerialNumber);
                }
                catch (Exception ex)
                {
                    // El log crítico es importante mantenerlo.
                    _logger.LogCritical(ex, "FALLO CRÍTICO: No se pudo persistir la actividad del dispositivo {SN}.", activity.SerialNumber);
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

                    _logger.LogInformation("Evento de servicio '{EventType}' persistido correctamente.", serviceEvent.EventType);
                }
                catch (Exception ex)
                {
                    // Se registra un error si la persistencia del evento falla.
                    _logger.LogError(ex, "FALLO al persistir el evento de servicio '{EventType}'.", serviceEvent.EventType);
                    // NOTA: No re-lanzamos la excepción ya que el evento debe continuar su flujo.
                }
            }
        }
    }
}
