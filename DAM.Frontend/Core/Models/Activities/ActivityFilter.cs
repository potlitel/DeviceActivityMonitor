using DAM.Frontend.Core.Models.Common;

/// <summary>
/// 📱 Filtro especializado para la gestión de dispositivos/actividades.
/// Hereda la lógica de paginación de <see cref="BaseFilter"/>.
/// </summary>
/// <param name="SerialNumber">Número de serie único del dispositivo.</param>
/// <param name="Status">Estado actual de la actividad (ej. Activo, Inactivo, Error).</param>
/// <param name="PageNumber">Número de página para el set de resultados.</param>
/// <param name="PageSize">Cantidad de elementos por página.</param>
public record ActivityFilter(
    string? SerialNumber = null,
    string? Status = null,
    int PageNumber = 1,
    int PageSize = 10
) : BaseFilter(PageNumber, PageSize);
