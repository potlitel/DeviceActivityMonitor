using DAM.Core.DTOs.Common;
using FluentValidation;
using System.Diagnostics.Tracing;

/// <summary>
/// Filtro para eventos del servicio con criterios de severidad y origen.
/// </summary>
/// <remarks>
/// Incluye restricciones de seguridad para prevenir filtrado por fuentes sensibles.
/// </remarks>
/// <param name="Level">Nivel de severidad del evento (opcional).</param>
/// <param name="Source">Fuente/origen del evento (opcional, con restricciones).</param>
/// <param name="PageNumber">Número de página.</param>
/// <param name="PageSize">Tamaño de página.</param>
public record ServiceEventFilter(EventLevel? Level, string? Source, int PageNumber, int PageSize)
    : PaginationRequest(PageNumber, PageSize);

/// <summary>
/// Validador para filtros de eventos del servicio.
/// </summary>
/// <remarks>
/// Aplica reglas de seguridad para prevenir exposición de información sensible.
/// </remarks>
public class ServiceEventFilterValidator : PaginationValidator<ServiceEventFilter>
{
    public ServiceEventFilterValidator()
    {
        RuleFor(x => x.Source)
            .Must(s => s != "Admin").WithMessage("No se permite filtrar por la fuente 'Admin' por seguridad.")
            .When(x => !string.IsNullOrEmpty(x.Source));
    }
}