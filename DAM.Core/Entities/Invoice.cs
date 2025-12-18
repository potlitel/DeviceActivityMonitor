namespace DAM.Core.Entities
{
    /// <summary>
    /// Entidad para registrar facturas relacionadas con la entidad DeviceActivity.
    /// </summary>
    public class Invoice
    {
        public int Id { get; set; }

        public required string SerialNumber { get; set; }

        public required DateTime Timestamp { get; set; }

        public required decimal TotalAmount { get; set; }

        public required string Description { get; set; }

        public int DeviceActivityId { get; set; }

        public DeviceActivity DeviceActivity { get; set; } = null!;
    }
}
