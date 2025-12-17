namespace DAM.Core.Constants
{
    public static class DataConstants
    {
        // Identificadores de Error y Estado
        public const string UnknownError = "UNKNOWN_ERR";
        public const string UnknownWmi = "UNKNOWN_WMI";
        public const string NotAvailable = "N/A";
        public const string WmiFailPrefix = "WMI_FAIL_";
        public const string PnpPrefix = "PNP_";

        // Configuración por Defecto
        public const string DefaultDbName = "DeviceActivityMonitor.db";
        public const string DefaultApiUrl = "http://localhost:5000/";
        public const string HealthEndpoint = "health";

        // Factores de Conversión Matemática
        // Definirlo como long evita castings repetitivos en los cálculos de bytes
        public const long BytesToMbFactor = 1024 * 1024;

        // Nombres de Secciones de appsettings.json
        // Esto evita errores de dedo al usar configuration.GetSection(...)
        public static class ConfigSections
        {
            public const string InvoiceSettings = "InvoiceSettings";
            public const string StorageSettings = "StorageSettings";
            public const string ApiSettings = "ApiSettings";
            public const string Database = "Database";
        }

        // Tiempos y Retrasos (Valores por defecto si no están en config)
        public static class Timeouts
        {
            public const int DefaultWatcherRecoveryMs = 5000;
            public const int DefaultApiCheckSeconds = 5;
            public const int DefaultRecheckIntervalMinutes = 1;
        }
    }
}
