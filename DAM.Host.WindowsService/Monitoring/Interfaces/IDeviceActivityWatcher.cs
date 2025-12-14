using DAM.Core.Entities;

namespace DAM.Host.WindowsService.Monitoring.Interfaces
{
    // Hereda de IDisposable, ya que el Worker lo llama.
    public interface IDeviceActivityWatcher : IDisposable
    {
        // El evento que el Worker suscribe.
        event Action<DeviceActivity> ActivityCompleted;

        // Propiedad para obtener los datos actuales (usada en tu prueba E2E).
        DeviceActivity CurrentActivity { get; }

        // Método que el Worker llama al desconectarse.
        void FinalizeActivity();

        // Asumiendo que hay un método para iniciar el monitoreo.
        //void StartMonitoring();
    }
}
