using DAM.Core.Interfaces;
using System.Management;

namespace DAM.Host.WindowsService.Monitoring
{
    // Implementación usando WMI (Windows Management Instrumentation) para eventos USB.
    public class WmiDeviceMonitor : IDeviceMonitor, IDisposable
    {
        public event Action<string>? DeviceConnected;
        public event Action<string>? DeviceDisconnected;

        // WMI Query Language (WQL) para detectar inserción de dispositivos (DriveType 2=Removable)
        private const string DeviceConnectQuery = "SELECT * FROM __InstanceCreationEvent WITHIN 2 " +
            "WHERE TargetInstance ISA 'Win32_LogicalDisk' AND TargetInstance.DriveType = 2";

        // WQL para detectar remoción de dispositivos
        private const string DeviceDisconnectQuery = "SELECT * FROM __InstanceDeletionEvent WITHIN 2 " +
            "WHERE TargetInstance ISA 'Win32_LogicalDisk' AND TargetInstance.DriveType = 2";

        private ManagementEventWatcher? _connectWatcher;
        private ManagementEventWatcher? _disconnectWatcher;

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

        // Implementación de IDisposable para liberar recursos de WMI
        public void Dispose()
        {
            _connectWatcher?.Dispose();
            _disconnectWatcher?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
