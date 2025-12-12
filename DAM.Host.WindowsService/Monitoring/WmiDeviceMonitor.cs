using DAM.Core.Interfaces;
using System.Management;

namespace DAM.Host.WindowsService.Monitoring
{
    /// <summary>
    /// Implementación del monitoreo de eventos de hardware de Windows utilizando WMI (Windows Management Instrumentation).
    /// </summary>
    /// <remarks>
    /// Detecta la inserción y remoción de unidades lógicas (DriveType 2 = Removable).
    /// </remarks>
    public class WmiDeviceMonitor : IDeviceMonitor, IDisposable
    {
        /// <inheritdoc/>
        public event Action<string>? DeviceConnected;

        /// <inheritdoc/>
        public event Action<string>? DeviceDisconnected;

        // WMI Query Language (WQL) para detectar inserción de dispositivos (DriveType 2=Removable)
        private const string DeviceConnectQuery = "SELECT * FROM __InstanceCreationEvent WITHIN 2 " +
            "WHERE TargetInstance ISA 'Win32_LogicalDisk' AND TargetInstance.DriveType = 2";

        // WQL para detectar remoción de dispositivos
        private const string DeviceDisconnectQuery = "SELECT * FROM __InstanceDeletionEvent WITHIN 2 " +
            "WHERE TargetInstance ISA 'Win32_LogicalDisk' AND TargetInstance.DriveType = 2";

        private ManagementEventWatcher? _connectWatcher;
        private ManagementEventWatcher? _disconnectWatcher;

        /// <inheritdoc/>
        public void StartMonitoring()
        {
            // Observador de conexión
            _connectWatcher = new ManagementEventWatcher(new WqlEventQuery(DeviceConnectQuery));
            _connectWatcher.EventArrived += OnDeviceConnected;
            _connectWatcher.Start();

            // Observador de desconexión
            _disconnectWatcher = new ManagementEventWatcher(new WqlEventQuery(DeviceDisconnectQuery));
            _disconnectWatcher.EventArrived += OnDeviceDisconnected;
            _disconnectWatcher.Start();
        }

        /// <inheritdoc/>
        public void StopMonitoring()
        {
            _connectWatcher?.Stop();
            _disconnectWatcher?.Stop();
            Dispose();
        }

        /// <summary>
        /// Maneja el evento WMI que indica la conexión de un nuevo dispositivo de almacenamiento.
        /// </summary>
        /// <param name="sender">La fuente del evento (normalmente el observador WMI).</param>
        /// <param name="e">Argumentos del evento WMI que contienen la información del nuevo dispositivo.</param>
        /// <remarks>
        /// Este método extrae la letra de unidad (ej. "E:") del objeto de instancia WMI ('TargetInstance')
        /// y dispara el evento <see cref="DeviceConnected"/> de la clase principal para notificar
        /// a los suscriptores.
        /// </remarks>
        private void OnDeviceConnected(object sender, EventArrivedEventArgs e)
        {
            var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            var driveLetter = instance["Name"]?.ToString(); // Ej: "E:"

            if (driveLetter != null)
            {
                DeviceConnected?.Invoke(driveLetter);
            }
        }

        /// <summary>
        /// Maneja el evento WMI que indica la desconexión de un dispositivo de almacenamiento existente.
        /// </summary>
        /// <param name="sender">La fuente del evento (normalmente el observador WMI).</param>
        /// <param name="e">Argumentos del evento WMI que contienen la información del dispositivo desconectado.</param>
        /// <remarks>
        /// Al igual que en <see cref="OnDeviceConnected"/>, este método obtiene la letra de unidad
        /// y utiliza el evento <see cref="DeviceDisconnected"/> para informar a los componentes 
        /// que estaban monitoreando esa unidad sobre la retirada.
        /// </remarks>
        private void OnDeviceDisconnected(object sender, EventArrivedEventArgs e)
        {
            var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            // Aquí obtenemos el nombre de la unidad que se desconectó.
            var driveLetter = instance["Name"]?.ToString();

            if (driveLetter != null)
            {
                DeviceDisconnected?.Invoke(driveLetter);
            }
        }

        /// <summary>
        /// Libera los recursos de WMI.
        /// </summary>
        public void Dispose()
        {
            _connectWatcher?.Dispose();
            _disconnectWatcher?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
