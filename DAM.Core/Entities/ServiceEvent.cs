namespace DAM.Core.Entities
{
    // Entidad para registrar eventos del servicio (como la detención manual).
    public class ServiceEvent
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty; // Ej: "START", "STOP", "MANUAL_STOP_ATTEMPT"
        public string Message { get; set; } = string.Empty;
    }
}
