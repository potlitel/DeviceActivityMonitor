using DAM.Core.DTOs.Common;
using FluentValidation;

/// <summary>
/// Filtro para consultas de auditoría con capacidades de paginación.
/// </summary>
/// <remarks>
/// <para>
/// Este filtro permite buscar registros de auditoría por usuario y rango de fechas,
/// implementando el patrón Specification para consultas flexibles.
/// </para>
/// <para>
/// <b>Comportamiento por defecto:</b>
/// <list type="bullet">
/// <item><description>Si Username es null: No filtra por usuario</description></item>
/// <item><description>Si FromDate es null: Trae registros desde el inicio</description></item>
/// <item><description>PageSize máximo: 100 registros por página</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Username">Nombre de usuario a filtrar (opcional).</param>
/// <param name="FromDate">Fecha de inicio para el filtro (opcional).</param>
/// <param name="PageNumber">Número de página (1-indexed).</param>
/// <param name="PageSize">Cantidad de registros por página.</param>
/// <example>
/// <code>
/// // Buscar auditorías del usuario "admin" en la última semana
/// var filter = new AuditFilter(
///     Username: "admin",
///     FromDate: DateTime.UtcNow.AddDays(-7),
///     PageNumber: 1,
///     PageSize: 20
/// );
/// </code>
/// </example>
public record AuditFilter(string? Username, DateTime? FromDate, int PageNumber, int PageSize)
    : PaginationRequest(PageNumber, PageSize);

/// <summary>
/// Validador para <see cref="AuditFilter"/> que extiende las reglas de paginación.
/// </summary>
/// <remarks>
/// Aplica validaciones específicas de dominio para filtros de auditoría:
/// <list type="bullet">
/// <item><description>La fecha de inicio no puede ser futura (consistencia temporal)</description></item>
/// <item><description>Hereda todas las validaciones de <see cref="PaginationValidator{T}"/></description></item>
/// </list>
/// </remarks>
public class AuditFilterValidator : PaginationValidator<AuditFilter>
{
    public AuditFilterValidator()
    {
        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("La fecha de inicio no puede ser futura.");
    }
}