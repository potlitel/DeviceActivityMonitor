using DAM.Frontend.Core.Models.Common;

namespace DAM.Frontend.Core.Models.Invoices
{
    /// <summary>
    /// 💰 Filtro para facturación.
    /// Optimizado para búsquedas por umbrales de montos económicos.
    /// </summary>
    public record InvoiceFilter(
        decimal? MinAmount = null,
        int PageNumber = 1,
        int PageSize = 10) : BaseFilter(PageNumber, PageSize);
}
