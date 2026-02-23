using DAM.Core.DTOs.Heartbeat;
using DAM.Core.Entities;
using DAM.Core.Interfaces;

namespace DAM.Infrastructure.Storage
{
    /// <summary>
    /// Implementación del servicio de almacenamiento que escribe los datos de actividad directamente a la BD local (SQLite).
    /// </summary>
    public class LocalDbStorageService : IActivityStorageService
    {
        //private readonly IActivityRepository _repository;
        private readonly IUnitOfWork _uow;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="LocalDbStorageService"/>.
        /// </summary>
        /// <param name="repository">Repositorio de actividad que maneja la persistencia con EF Core.</param>
        public LocalDbStorageService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        /// <inheritdoc/>
        public Task StoreActivityAsync(DeviceActivity activity)
        {
            return _uow.Activities.AddActivityAsync(activity);
        }

        /// <inheritdoc/>
        public Task StoreDevicePresenceAsync(DevicePresence presence)
        {
            return _uow.Activities.AddDevicePresenceAsync(presence);
        }

        /// <inheritdoc/>
        public Task StoreInvoiceAsync(Invoice invoice)
        {
            return _uow.Activities.AddInvoiceAsync(invoice);
        }

        /// <inheritdoc/>
        public Task StoreServiceEventAsync(ServiceEvent serviceEvent)
        {
            return _uow.Activities.AddServiceEventAsync(serviceEvent);
        }

        /// <inheritdoc/>
        public Task UpdateActivityAsync(DeviceActivity activity)
        {
            return _uow.Activities.UpdateActivityAsync(activity);
        }

        public Task SendHeartbeatAsync(HeartbeatDto heartbeat)
        {
            // Implementación vacía intencional (Pattern: No-Op)
            return Task.CompletedTask;
        }

        public Task BeginTransactionAsync() => _uow.BeginTransactionAsync();
        public Task CommitTransactionAsync() => _uow.CommitTransactionAsync();
        public Task RollbackTransactionAsync() => _uow.RollbackTransactionAsync();
        public Task SaveChangesAsync() => _uow.SaveChangesAsync();
    }
}
