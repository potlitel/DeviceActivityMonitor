using DAM.Core.Entities;

namespace DAM.Core.Interfaces
{
    // Define el contrato para guardar datos, independientemente de si es local o remoto.
    public interface IActivityStorageService
    {
        Task StoreActivityAsync(DeviceActivity activity);
        Task StoreServiceEventAsync(ServiceEvent serviceEvent);
    }
}
