namespace DAM.Core.Entities
{
    /// <summary>
    /// Entidad para registrar el historial de presencia de los dispositivos externos.
    /// </summary>
    public class DevicePresence
    {
        public int Id { get; set; }

        public required string SerialNumber { get; set; }

        // La hora exacta en que se observó la presencia (Fecha + Hora)
        // Esto permite múltiples registros por día.
        public required DateTime Timestamp { get; set; }
    }
}
