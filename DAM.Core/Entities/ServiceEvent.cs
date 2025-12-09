namespace DAM.Core.Entities
{
    /// <summary>
    /// Entidad para registrar eventos internos del servicio de Windows (ej. inicio, parada, intentos de detención).
    /// </summary>
    public class ServiceEvent
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty; // Ej: "START", "STOP", "MANUAL_STOP_ATTEMPT"
        public string Message { get; set; } = string.Empty;
    }
}
