namespace DAM.Core.Constants
{
    public static class Messages
    {
        public static class Watcher
        {
            public const string DriveNotReady = "Drive {Drive} not ready upon insertion. Activity logging may be incomplete.";
            public const string WatcherCreationError = "Error al crear FileSystemWatcher para la unidad {DriveLetter}. El monitoreo de E/S no estará activo.";
            public const string IoErrorFatal = "Error I/O fatal en FileSystemWatcher para {DriveLetter}. Intentando recuperar.";
            public const string RecoverySuccess = "FileSystemWatcher para {DriveLetter} recuperado exitosamente.";
            public const string RecoveryFailed = "Fallo al recuperar FileSystemWatcher para {DriveLetter}. Monitoreo de E/S perdido.";
            public const string FileMissing = "El archivo fue creado pero ya no existe en el momento del acceso I/O. Se ignora la actividad: {Path}";
            public const string IoAccessError = "Fallo de I/O al acceder al archivo creado. Se ignorará esta actividad: {Path}";
            public const string UnexpectedError = "Error inesperado al procesar evento: {Path}";
        }

        public static class Persistence
        {
            public const string PresenceSaved = "Conexión registrada en BD para {SN}.";
            public const string PresenceFailed = "FALLO: No se pudo registrar la presencia del dispositivo {SN} al conectar.";
            public const string ActivitySaved = "Actividad del dispositivo {SN} persistida exitosamente.";
            public const string ActivityCritical = "FALLO CRÍTICO: No se pudo persistir la actividad del dispositivo {SN}.";
            public const string EventSaved = "Evento de servicio '{EventType}' persistido correctamente.";
            public const string EventFailed = "FALLO al persistir el evento de servicio '{EventType}'.";
            public const string InvoiceSaved = "Factura de {Monto:C} calculada y persistida para {SN}.";
            public const string InvoiceFailed = "FALLO CRÍTICO: No se pudo persistir la factura para el dispositivo {SN}.";
        }

        public static class Invoice
        {
            public const string DescriptionFormat = "Factura por {0} archivo(s) neto(s) (Copiados: {1} - Eliminados: {2}). Costo: {3:C} c/u.";
        }

        public static class Repository
        {
            public const string SaveActivityError = "Error al guardar DeviceActivity en la BD.";
            public const string SavePresenceError = "Error al guardar DevicePresence en la BD.";
            public const string SaveInvoiceError = "Error al guardar Invoice en la BD.";
            public const string SaveEventError = "Error al guardar ServiceEvent en la BD.";
            public const string MigrationError = "Ocurrió un error durante la migración de la base de datos.";
        }

        public static class Storage
        {
            public const string UsingWebApi = "Usando estrategia: Web API.";
            public const string UsingLocalSqlite = "Usando estrategia: SQLite Local. Web API no disponible.";
        }

        public static class Api
        {
            public const string NotAvailable = "Web API no disponible. Falló la conexión: {Message}";
            public const string UnknownError = "Error desconocido al verificar la API.";
        }
    }
}
