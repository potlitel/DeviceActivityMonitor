namespace DAM.Core.DTOs.Invoices;

/// <summary>
/// Respuesta detallada de facturación con métricas de procesamiento.
/// </summary>
/// <remarks>
/// <para>
/// Incluye información cuantitativa sobre el procesamiento de datos
/// para correlacionar costos con utilización real.
/// </para>
/// <para>
/// <b>Cálculo de costos:</b> El monto total puede derivarse de:
/// <c>TotalAmount = BaseRate + (MegabytesProcessed * RatePerMB)</c>
/// </para>
/// </remarks>
/// <param name="InvoiceNumber">Número único de factura.</param>
/// <param name="SerialNumber">Número de serie del dispositivo.</param>
/// <param name="TotalAmount">Monto total facturado.</param>
/// <param name="MegabytesProcessed">Megabytes procesados durante el período.</param>
/// <param name="GeneratedAt">Fecha de generación.</param>
public record InvoiceResponse(
    string InvoiceNumber,
    string SerialNumber,
    decimal TotalAmount,
    double MegabytesProcessed,
    DateTime GeneratedAt
);