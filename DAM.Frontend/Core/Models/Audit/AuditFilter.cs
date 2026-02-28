using DAM.Frontend.Core.Models.Common;

namespace DAM.Frontend.Core.Models.Audit
{
    /// <summary>
    /// 📑 Filtro para auditoría de acciones de usuario.
    /// Permite rastrear quién hizo qué y en qué rango de fechas.
    /// </summary>
    public record AuditFilter(
        string? Username = null,
        DateTime? FromDate = null,
        DateTime? ToDate = null,
        int PageNumber = 1,
        int PageSize = 10) : BaseFilter(PageNumber, PageSize);
}
