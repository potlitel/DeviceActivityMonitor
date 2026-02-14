using FluentValidation;

namespace DAM.Core.DTOs.Common;

/// <summary>
/// Solicitud base con parámetros de paginación para consultas paginadas.
/// </summary>
/// <remarks>
/// <para>
/// Implementa el patrón Offset-based Pagination para navegación secuencial de resultados.
/// Ideal para tablas con datos que no cambian frecuentemente.
/// </para>
/// <para>
/// <b>Alternativas consideradas:</b>
/// <list type="bullet">
/// <item><description><b>Cursor-based Pagination</b>: Mejor para datasets grandes y frecuentes cambios</description></item>
/// <item><description><b>Keyset Pagination</b>: Óptimo para rendimiento con índices compuestos</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="PageNumber">Número de página (base 1).</param>
/// <param name="PageSize">Cantidad de registros por página (1-100).</param>
/// <example>
/// <code>
/// // Primera página con 20 registros
/// var request = new PaginationRequest(PageNumber: 1, PageSize: 20);
/// 
/// // Usando valores por defecto (página 1, 10 registros)
/// var defaultRequest = new PaginationRequest();
/// </code>
/// </example>
public record PaginationRequest(int PageNumber = 1, int PageSize = 10);

/// <summary>
/// Validador base genérico para solicitudes de paginación.
/// </summary>
/// <typeparam name="T">Tipo de solicitud que hereda de <see cref="PaginationRequest"/>.</typeparam>
/// <remarks>
/// Proporciona validaciones consistentes para todos los filtros paginados:
/// <list type="number">
/// <item><description>PageNumber ≥ 1 (evita errores de offset negativo)</description></item>
/// <item><description>PageSize entre 1 y 100 (balance entre rendimiento y usabilidad)</description></item>
/// </list>
/// <para>
/// <b>Mejora de rendimiento:</b> Limitar PageSize a 100 previene consultas excesivamente grandes
/// que podrían afectar el rendimiento de la base de datos.
/// </para>
/// </remarks>
public class PaginationValidator<T> : AbstractValidator<T> where T : PaginationRequest
{
    public PaginationValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("La página debe ser mayor o igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100 registros.");
    }
}