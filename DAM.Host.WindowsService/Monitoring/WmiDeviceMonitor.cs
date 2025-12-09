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

        private void OnDeviceConnected(object sender, EventArrivedEventArgs e)
        {
            var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            var driveLetter = instance["Name"]?.ToString(); // Ej: "E:"

            if (driveLetter != null)
            {
                DeviceConnected?.Invoke(driveLetter);
            }
        }

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
