namespace DAM.Core.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Action { get; set; } = null!; // Path de la API
        public string Resource { get; set; } = null!; // Nombre del Endpoint
        public string HttpMethod { get; set; } = null!;
        public string IpAddress { get; set; } = null!;
        public DateTime TimestampUtc { get; set; }
    }
}
