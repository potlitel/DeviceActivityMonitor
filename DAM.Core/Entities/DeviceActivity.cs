namespace DAM.Core.Entities
{
    /// <summary>
    /// Representa el registro de la actividad completa de un dispositivo de almacenamiento externo 
    /// (ej. USB) durante una sesión de conexión a la estación de trabajo.
    /// </summary>
    public class DeviceActivity
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty; // Clave única del dispositivo
        public string Model { get; set; } = string.Empty;
        public long TotalCapacityMB { get; set; }
        public DateTime InsertedAt { get; set; }
        public DateTime? ExtractedAt { get; set; }
        public long InitialAvailableMB { get; set; } // Capacidad disponible al insertarse
        public long FinalAvailableMB { get; set; } // Capacidad disponible al extraerse

        // Métricas de la Sesión
        public long MegabytesCopied { get; set; }
        public long MegabytesDeleted { get; set; }
        public TimeSpan TimeInserted => (ExtractedAt ?? DateTime.Now) - InsertedAt;

        // Historial de Archivos
        public List<string> FilesCopied { get; set; } = new List<string>();
        public List<string> FilesDeleted { get; set; } = new List<string>();

        // Eventos especiales (Formateo, etc.)
        public string SpecialEvent { get; set; } = string.Empty;

        // Historial de Presencia (para saber los días que ha venido anteriormente)
        // Esto se manejaría como una query aparte o en la misma tabla con un Distinct en la fecha.
    }
}
