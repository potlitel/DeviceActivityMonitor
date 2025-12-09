namespace DAM.Core.Interfaces
{
    /// <summary>
    /// Contrato para monitorear los eventos de conexión y desconexión de dispositivos externos a nivel de sistema operativo (ej. WMI).
    /// </summary>
    public interface IDeviceMonitor
    {
        /// <summary>
        /// Evento disparado cuando se conecta un nuevo dispositivo.
        /// </summary>
        /// <remarks>Recibe la ruta de la unidad (ej: "E:\").</remarks>
        event Action<string> DeviceConnected;

        /// <summary>
        /// Evento disparado cuando se desconecta un dispositivo existente.
        /// </summary>
        /// <remarks>Recibe la ruta de la unidad que se desconectó (ej: "E:\").</remarks>
        event Action<string> DeviceDisconnected;

        /// <summary>
        /// Inicia el monitoreo de eventos de conexión/desconexión de dispositivos.
        /// </summary>
        void StartMonitoring();

        /// <summary>
        /// Detiene el monitoreo de eventos de conexión/desconexión de dispositivos y libera recursos.
        /// </summary>
        void StopMonitoring();
    }
}
