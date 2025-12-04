namespace DAM.Core.Interfaces
{
    // Define el contrato para monitorear eventos de conexión/desconexión de dispositivos.
    public interface IDeviceMonitor
    {
        event Action<string> DeviceConnected; // Recibe la ruta de la unidad (ej: "E:\")
        event Action<string> DeviceDisconnected; // Recibe la ruta de la unidad
        void StartMonitoring();
        void StopMonitoring();
    }
}
