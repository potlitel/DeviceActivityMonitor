using DAM.Core.Constants;
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
        private readonly ILogger<WmiDeviceMonitor> _logger;
        private ManagementEventWatcher? _connectWatcher;
        private ManagementEventWatcher? _disconnectWatcher;

        /// <inheritdoc/>
        public event Action<string>? DeviceConnected;

        /// <inheritdoc/>
        public event Action<string>? DeviceDisconnected;

        public WmiDeviceMonitor(ILogger<WmiDeviceMonitor> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public void StartMonitoring()
        {
            try
            {
                _connectWatcher = new ManagementEventWatcher(new WqlEventQuery(WmiQueries.DeviceConnect));
                _connectWatcher.EventArrived += OnDeviceConnected;
                _connectWatcher.Start();

                _disconnectWatcher = new ManagementEventWatcher(new WqlEventQuery(WmiQueries.DeviceDisconnect));
                _disconnectWatcher.EventArrived += OnDeviceDisconnected;
                _disconnectWatcher.Start();

                _logger.LogInformation(Messages.Wmi.StartSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, Messages.Wmi.StartCritical);
                throw;
            }
        }

        /// <inheritdoc/>
        public void StopMonitoring()
        {
            _logger.LogInformation(Messages.Wmi.StopInfo);
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
            ProcessEvent(e, DeviceConnected, "Conexión");
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
            ProcessEvent(e, DeviceDisconnected, "Desconexión");
        }

        /// <summary>
        /// Realiza el parsing del evento WMI entrante, extrae los metadatos de la instancia 
        /// y notifica a los suscriptores del evento correspondiente.
        /// </summary>
        /// <param name="e">Argumentos del evento generado por WMI que contienen la estructura 'TargetInstance'.</param>
        /// <param name="action">El delegado (evento) que debe ser invocado tras la validación exitosa.</param>
        /// <param name="eventType">Etiqueta descriptiva del tipo de evento para propósitos de trazabilidad y diagnóstico.</param>
        /// <remarks>
        /// Este método actúa como un puente de normalización entre los objetos dinámicos de WMI 
        /// y la lógica de negocio fuertemente tipada de la aplicación. 
        /// Se asegura de filtrar valores nulos y capturar excepciones de acceso a propiedades de gestión.
        /// </remarks>
        private void ProcessEvent(EventArrivedEventArgs e, Action<string>? action, string eventType)
        {
            try
            {
                var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
                var driveLetter = instance["Name"]?.ToString(); // Ej: "E:"

                if (!string.IsNullOrEmpty(driveLetter))
                {
                    _logger.LogDebug(Messages.Wmi.EventDetected, eventType, driveLetter);
                    action?.Invoke(driveLetter);
                }
                else
                {
                    _logger.LogWarning(Messages.Wmi.InvalidDrive, eventType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Messages.Wmi.ProcessError, eventType);
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
