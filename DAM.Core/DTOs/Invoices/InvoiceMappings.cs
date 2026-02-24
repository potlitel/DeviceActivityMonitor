using DAM.Core.Features.Invoices.Commands;

namespace DAM.Core.DTOs.Invoices
{
    /// <summary>
    /// Proporciona métodos de extensión para mapear comandos de facturación a entidades <see cref="Invoice"/>.
    /// </summary>
    /// <remarks>
    /// Las facturas en el sistema están siempre asociadas a una <see cref="DeviceActivity"/> específica
    /// y representan cargos por uso, copia de datos, o servicios prestados durante la sesión del dispositivo.
    /// </remarks>
    public static class InvoiceMappings
    {
        /// <summary>
        /// Convierte un comando de creación de factura en una entidad <see cref="Invoice"/>.
        /// </summary>
        /// <param name="cmd">El comando con los datos de la factura a generar.</param>
        /// <returns>Una nueva instancia de <see cref="Invoice"/> lista para persistir.</returns>
        /// <remarks>
        /// La entidad <see cref="Invoice"/> resultante:
        /// <list type="bullet">
        /// <item><description>Está asociada a una actividad específica mediante <see cref="Invoice.DeviceActivityId"/></description></item>
        /// <item><description>Contiene el monto calculado (<see cref="Invoice.TotalAmount"/>)</description></item>
        /// <item><description>Incluye una descripción detallada del concepto facturado</description></item>
        /// </list>
        /// <para>
        /// <b>Nota importante:</b> Este método solo realiza el mapeo de datos. El cálculo del monto
        /// de la factura debe realizarse en la capa de negocio antes de invocar este mapeo.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Se lanza si <paramref name="cmd"/> es null.</exception>
        /// <example>
        /// Uso típico después del cálculo de negocio:
        /// <code>
        /// // Calcular monto (capa de negocio)
        /// var amount = invoiceCalculator.Calculate(activity);
        /// 
        /// var cmd = new CreateInvoiceCmd(
        ///     activity.SerialNumber,
        ///     DateTime.UtcNow,
        ///     amount,
        ///     "Factura por servicios",
        ///     activity.Id
        /// );
        /// 
        /// var invoice = cmd.ToEntity();
        /// await uow.Activities.AddInvoiceAsync(invoice);
        /// </code>
        /// </example>
        public static Entities.Invoice ToEntity(this CalculateInvoiceCommand cmd)
        {
            return new Entities.Invoice
            {
                SerialNumber = cmd.SerialNumber,
                Timestamp = cmd.Timestamp,
                TotalAmount = cmd.TotalAmount,
                Description = cmd.Description,
                DeviceActivityId = cmd.DeviceActivityId
            };
        }
    }
}
