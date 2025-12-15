using DAM.Core.Entities;

namespace DAM.Core.Interfaces
{
    public interface IDevicePresenceRepository
    {
        /// <summary>
        /// Registra la presencia de un dispositivo en la hora específica.
        /// </summary>
        Task RegisterPresenceAsync(DevicePresence presence);

        Task<IEnumerable<DateTime>> GetPresenceHistoryAsync(string serialNumber);
    }
}
