using System.Text.Json.Serialization;

namespace DAM.Frontend.Core.Models.Invoices
{
    public record InvoiceDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("serialNumber")] string SerialNumber,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("totalAmount")] decimal TotalAmount,
    [property: JsonPropertyName("description")] string Description);
}
