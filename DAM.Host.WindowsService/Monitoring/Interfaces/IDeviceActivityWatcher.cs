using DAM.Core.Entities;

namespace DAM.Host.WindowsService.Monitoring.Interfaces
{
    /// <summary>
    /// Define el contrato para el objeto autónomo encargado de monitorear la actividad de E/S
    /// (creación, modificación, eliminación de archivos) en un dispositivo de almacenamiento.
    /// 
    /// Hereda de IDisposable para garantizar que los recursos del sistema operativo
    /// (como FileSystemWatcher y conexiones WMI) sean liberados explícitamente por el Worker
    /// cuando el dispositivo es desconectado.
    /// </summary>
    public interface IDeviceActivityWatcher : IDisposable
    {
        // El evento que el Worker suscribe.
        /// <summary>
        /// Evento disparado cuando la recolección de datos de un dispositivo ha finalizado
        /// (generalmente tras la desconexión o al ser forzado).
        /// El Worker debe suscribirse a este evento para recibir y persistir los datos finales.
        /// </summary>
        event Action<DeviceActivity> ActivityCompleted;

        /// <summary>
        /// Obtiene la instancia actual de DeviceActivity que se está recolectando.
        /// Contiene los metadatos iniciales y los contadores acumulados de E/S.
        /// </summary>
        DeviceActivity CurrentActivity { get; }

        /// <summary>
        /// Finaliza el monitoreo de actividad, registra la hora de finalización
        /// y dispara el evento <see cref="ActivityCompleted"/>.
        /// Este método debe ser llamado por el Worker cuando el dispositivo se desconecta.
        /// </summary>
        void FinalizeActivity();
    }
}
