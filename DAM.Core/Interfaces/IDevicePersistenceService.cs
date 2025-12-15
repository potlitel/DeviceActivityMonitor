using DAM.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Core.Interfaces
{
    public interface IDevicePersistenceService // Nuevo nombre más sugerente
    {
        // Mantiene la funcionalidad de registro de presencia (HandleDeviceConnected)
        Task PersistPresenceAsync(string serialNumber);

        // NUEVA FUNCIONALIDAD: Almacena la actividad completada (HandleActivityCompleted)
        Task PersistActivityAsync(DeviceActivity activity);
    }
}
