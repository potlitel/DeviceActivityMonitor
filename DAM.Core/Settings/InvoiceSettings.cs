namespace DAM.Core.Settings
{
    /// <summary>
    /// Representa los parámetros de configuración para el cálculo de facturación.
    /// </summary>
    /// <remarks>
    /// Estos valores son inyectados habitualmente desde la sección 'InvoiceSettings' del archivo appsettings.json.
    /// </remarks>
    public class InvoiceSettings
    {
        /// <summary>
        /// Obtiene o establece el precio unitario a cobrar por cada archivo procesado.
        /// </summary>
        /// <value>
        /// Un valor decimal que representa el costo (ej. 0.05 para 5 centavos por archivo).
        /// </value>
        public decimal PricePerFile { get; set; }
    }
}
