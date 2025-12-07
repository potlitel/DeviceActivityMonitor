using DAM.Core.Entities;

namespace DAM.Core.Interfaces
{
    public interface IActivityRepository
    {
        Task AddActivityAsync(DeviceActivity activity);
        Task AddServiceEventAsync(ServiceEvent serviceEvent);
        // ... otros métodos de consulta ...
    }
}
