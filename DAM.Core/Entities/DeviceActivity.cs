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

        public TimeSpan? TimeInserted { get; private set; }

        public TimeSpan CalculatedDuration => (ExtractedAt ?? DateTime.UtcNow) - InsertedAt;

        // Historial de Archivos
        public List<string> FilesCopied { get; set; } = new List<string>();
        public List<string> FilesDeleted { get; set; } = new List<string>();

        // Eventos especiales (Formateo, etc.)
        public string SpecialEvent { get; set; } = string.Empty;

        public ICollection<DevicePresence> PresenceHistory { get; set; } = [];

        public ICollection<Invoice> Invoices { get; set; } = [];

        /// <summary>
        /// Establece la duración final de la actividad del dispositivo.
        /// </summary>
        /// <remarks>
        /// Este método se invoca cuando una <c>DeviceActivity</c> ha finalizado, 
        /// independientemente de si las operaciones realizadas fueron de copiado/eliminado 
        /// o de solo lectura. 
        /// <para>
        /// La asignación solo ocurre si existe un valor previo en <see cref="ExtractedAt"/>, 
        /// asegurando la integridad de la métrica de tiempo.
        /// </para>
        /// </remarks>
        public void SetTimeInsertedDuration()
        {
            if (ExtractedAt.HasValue)
            {
                TimeInserted = CalculatedDuration;
            }
        }
    }
}
