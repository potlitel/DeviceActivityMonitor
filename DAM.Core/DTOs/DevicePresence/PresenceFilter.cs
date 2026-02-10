using DAM.Core.DTOs.Common;
using FluentValidation;

/// <summary>
/// Filtro para consultar eventos de presencia asociados a actividades.
/// </summary>
/// <param name="ActivityId">Identificador de la actividad para filtrar (opcional).</param>
/// <param name="PageNumber">Número de página.</param>
/// <param name="PageSize">Tamaño de página.</param>
public record PresenceFilter(Guid? ActivityId, int PageNumber, int PageSize)
    : PaginationRequest(PageNumber, PageSize);

/// <summary>
/// Validador para filtros de presencia.
/// </summary>
public class PresenceFilterValidator : PaginationValidator<PresenceFilter>
{
    public PresenceFilterValidator()
    {
        RuleFor(x => x.ActivityId)
            .NotEmpty().WithMessage("Si filtra por actividad, el ID no puede estar vacío.")
            .When(x => x.ActivityId.HasValue);
    }
}