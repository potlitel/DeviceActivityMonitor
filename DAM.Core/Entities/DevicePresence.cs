namespace DAM.Core.Entities
{
    public class DevicePresence
    {
        public int Id { get; set; }

        public required string SerialNumber { get; set; }

        // La hora exacta en que se observó la presencia (Fecha + Hora)
        // Esto permite múltiples registros por día.
        public required DateTime Timestamp { get; set; }
    }
}
