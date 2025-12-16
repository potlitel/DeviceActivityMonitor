namespace DAM.Core.Constants
{
    public static class WorkerMessages
    {
        // --- Tipos de Eventos del Servicio (Para la base de datos) ---
        public static class ServiceEventTypes
        {
            public const string ServiceStart = "SERVICE_START";
            public const string ServiceStop = "SERVICE_STOP";
        }

        // --- Mensajes de Eventos del Servicio (Para la base de datos) ---
        public static class ServiceEventMessages
        {
            public const string ServiceStarted = "El servicio Device Activity Monitor ha iniciado la ejecución.";
            public const string ServiceStopped = "El servicio Device Activity Monitor ha finalizado la ejecución.";
        }

        // --- Mensajes de Logging (Para el ILogger) ---
        public static class Log
        {
            public const string ServiceStarting = "DAM Worker Service starting at: {time}";
            public const string MonitoringStarted = "Device Monitoring Started.";
            public const string DeviceConnected = "Device connected: {DriveLetter}";
            public const string DeviceDisconnected = "Device disconnected: {DriveLetter}";
            public const string TotalConnectedDevices = "Total connected devices: {Count}";
            public const string ActivityProcessed = "Presencia y Factura inicial procesadas para {SN}.";
            public const string ActivityFailed = "FALLO: Error al orquestar la persistencia de conexión para {SN}.";
            public const string ActivityFinished = "Activity finished for {SN}. Time: {Time}";
            public const string ActivityInvoiceProcessed = "Actividad y Factura final del dispositivo {SN} persistidas exitosamente.";
            public const string ActivityInvoiceProcessedFailed = "FALLO CRÍTICO: No se pudo persistir la actividad o la factura del dispositivo {SN}.";
            public const string ServiceStopping = "DAM Worker Service is stopping.";
        }
    }
}
