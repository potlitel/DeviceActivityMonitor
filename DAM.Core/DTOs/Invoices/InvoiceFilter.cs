using DAM.Core.DTOs.Common;
using FluentValidation;

/// <summary>
/// Filtro para consultar facturas con parámetros financieros.
/// </summary>
/// <param name="MinAmount">Monto mínimo de facturación (opcional).</param>
/// <param name="PageNumber">Número de página.</param>
/// <param name="PageSize">Tamaño de página.</param>
public record InvoiceFilter(decimal? MinAmount, int PageNumber, int PageSize)
    : PaginationRequest(PageNumber, PageSize);

/// <summary>
/// Validador para filtros de facturas.
/// </summary>
public class InvoiceFilterValidator : PaginationValidator<InvoiceFilter>
{
    public InvoiceFilterValidator()
    {
        RuleFor(x => x.MinAmount)
            .GreaterThanOrEqualTo(0).WithMessage("El monto mínimo no puede ser negativo.")
            .When(x => x.MinAmount.HasValue);
    }
}