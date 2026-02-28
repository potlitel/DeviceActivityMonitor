using DAM.Frontend.Core.Models.Common;

namespace DAM.Frontend.Core.Models.Presence
{
    /// <summary>
    /// 📍 Filtro de presencia o asistencia.
    /// Vincula registros específicos a un identificador único de actividad.
    /// </summary>
    public record PresenceFilter(
        Guid? ActivityId = null,
        int PageNumber = 1,
        int PageSize = 10) : BaseFilter(PageNumber, PageSize);
}
