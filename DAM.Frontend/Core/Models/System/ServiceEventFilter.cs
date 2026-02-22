using DAM.Frontend.Core.Models.Common;

namespace DAM.Frontend.Core.Models.System
{
    /// <summary>
    /// 📊 Niveles de severidad para eventos de sistema.
    /// </summary>
    public enum EventLevel
    {
        Information,
        Warning,
        Error,
        Critical
    }

    /// <summary>
    /// 📡 Filtro para eventos y logs del servicio.
    /// Permite rastrear la salud del sistema filtrando por origen y severidad.
    /// </summary>
    /// <param name="Level">Severidad del evento (null para todos).</param>
    /// <param name="Source">Componente de origen (ej. "AuthService", "Billing").</param>
    /// <param name="PageNumber">Número de página (heredado de BaseFilter).</param>
    /// <param name="PageSize">Tamaño de página (heredado de BaseFilter).</param>
    public record ServiceEventFilter(
        EventLevel? Level = null,
        string? Source = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : BaseFilter(PageNumber, PageSize);
}
