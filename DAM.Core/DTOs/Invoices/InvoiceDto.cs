namespace DAM.Core.DTOs.Invoices
{
    /// <summary>
    /// DTO que representa una factura generada en el sistema.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Las facturas representan transacciones económicas asociadas al uso
    /// de dispositivos y procesamiento de datos.
    /// </para>
    /// <para>
    /// <b>Nota:</b> La propiedad DeviceActivity está pendiente de implementación
    /// para establecer la relación completa entre facturas y actividades.
    /// </para>
    /// </remarks>
    /// <param name="Id">Identificador único de la factura.</param>
    /// <param name="SerialNumber">Número de serie del dispositivo facturado.</param>
    /// <param name="Timestamp">Fecha de generación de la factura.</param>
    /// <param name="TotalAmount">Monto total de la factura.</param>
    /// <param name="Description">Descripción detallada de la factura.</param>
    public record InvoiceDto(
    int Id,
    string SerialNumber,
    DateTime Timestamp,
    decimal TotalAmount,
    string Description
    //TODO: Incluir la entidad DeviceActivity relacionada con cada factura!!!
);
}
