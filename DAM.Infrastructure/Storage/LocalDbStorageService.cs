using DAM.Core.Entities;
using DAM.Core.Interfaces;

namespace DAM.Infrastructure.Storage
{
    /// <summary>
    /// Implementación del servicio de almacenamiento que escribe los datos de actividad directamente a la BD local (SQLite).
    /// </summary>
    public class LocalDbStorageService : IActivityStorageService
    {
        private readonly IActivityRepository _repository;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="LocalDbStorageService"/>.
        /// </summary>
        /// <param name="repository">Repositorio de actividad que maneja la persistencia con EF Core.</param>
        public LocalDbStorageService(IActivityRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public Task StoreActivityAsync(DeviceActivity activity)
        {
            return _repository.AddActivityAsync(activity);
        }

        /// <inheritdoc/>
        public Task StoreDevicePresenceAsync(DevicePresence presence)
        {
            return _repository.AddDevicePresenceAsync(presence);
        }

        /// <inheritdoc/>
        public Task StoreInvoiceAsync(Invoice invoice)
        {
            return _repository.AddInvoiceAsync(invoice);
        }

        /// <inheritdoc/>
        public Task StoreServiceEventAsync(ServiceEvent serviceEvent)
        {
            return _repository.AddServiceEventAsync(serviceEvent);
        }

        /// <inheritdoc/>
        public Task UpdateActivityAsync(DeviceActivity activity)
        {
            return _repository.UpdateActivityAsync(activity);
        }
    }
}
