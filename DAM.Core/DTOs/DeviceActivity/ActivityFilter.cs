using DAM.Core.DTOs.Common;
using DAM.Core.Enums;
using FluentValidation;

/// <summary>
/// Filtro para consultar actividades de dispositivos con estado y paginación.
/// </summary>
/// <remarks>
/// Permite buscar actividades por número de serie y estado, útil para dashboards
/// de monitoreo y reportes operativos.
/// </remarks>
/// <param name="SerialNumber">Número de serie del dispositivo (opcional).</param>
/// <param name="Status">Estado de la actividad (opcional).</param>
/// <param name="PageNumber">Número de página.</param>
/// <param name="PageSize">Tamaño de página.</param>
/// <example>
/// <code>
/// // Buscar actividades activas de un dispositivo específico
/// var filter = new ActivityFilter(
///     SerialNumber: "SN-12345",
///     Status: ActivityStatus.Active,
///     PageNumber: 1,
///     PageSize: 50
/// );
/// </code>
/// </example>
public record ActivityFilter(string? SerialNumber, ActivityStatus? Status, int PageNumber, int PageSize)
    : PaginationRequest(PageNumber, PageSize);

/// <summary>
/// Validador para filtros de actividades de dispositivos.
/// </summary>
public class ActivityFilterValidator : PaginationValidator<ActivityFilter>
{
    public ActivityFilterValidator()
    {
        RuleFor(x => x.SerialNumber)
            .MaximumLength(50).WithMessage("El Serial Number no puede exceder los 50 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.SerialNumber));
    }
}