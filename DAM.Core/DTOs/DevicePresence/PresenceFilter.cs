using DAM.Core.DTOs.Common;
using FluentValidation;

/// <summary>
/// Filtro para consultar eventos de presencia asociados a actividades.
/// </summary>
/// <param name="ActivityId">Identificador de la actividad para filtrar (opcional).</param>
/// <param name="PageNumber">Número de página.</param>
/// <param name="PageSize">Tamaño de página.</param>
public record PresenceFilter(int? ActivityId, int PageNumber, int PageSize)
    : PaginationRequest(PageNumber, PageSize);

/// <summary>
/// Validador para filtros de presencia.
/// </summary>
public class PresenceFilterValidator : PaginationValidator<PresenceFilter>
{
    public PresenceFilterValidator()
    {
        // 🎯 1. REGLA DE DOMINIO: ActivityId debe ser positivo
        RuleFor(x => x.ActivityId)
            .Must(id => !id.HasValue || id.Value > 0)
            .WithMessage("❌ ActivityId debe ser un número entero POSITIVO.");

        // 🎯 2. REGLA DE DOMINIO: Límite razonable
        RuleFor(x => x.ActivityId)
            .Must(id => !id.HasValue || id.Value < 1_000_000)
            .WithMessage("⚠️ ActivityId excede el límite máximo permitido.");

        // 🎯 3. REGLA DE DOMINIO: Consistencia
        RuleFor(x => x)
            .Must(x => !x.ActivityId.HasValue || x.ActivityId.Value > 0)
            .WithMessage("Si proporciona ActivityId, debe ser un número entero válido.");
    }
}