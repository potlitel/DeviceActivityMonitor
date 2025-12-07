using DAM.Core.Entities;
using DAM.Core.Interfaces;

namespace DAM.Infrastructure.Storage
{
    // Escribe directamente a la BD local (SQLite) usando el repositorio.
    public class LocalDbStorageService : IActivityStorageService
    {
        private readonly IActivityRepository _repository;

        public LocalDbStorageService(IActivityRepository repository)
        {
            _repository = repository;
        }

        public Task StoreActivityAsync(DeviceActivity activity)
        {
            return _repository.AddActivityAsync(activity);
        }

        public Task StoreServiceEventAsync(ServiceEvent serviceEvent)
        {
            return _repository.AddServiceEventAsync(serviceEvent);
        }
    }
}
