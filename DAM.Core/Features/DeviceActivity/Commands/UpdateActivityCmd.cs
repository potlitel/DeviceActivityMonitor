using DAM.Core.Abstractions;
using DAM.Core.Enums;
using FluentValidation;

namespace DAM.Core.Features.DeviceActivity.Commands
{
    /// <summary>
    /// Comando para la actualización de una actividad existente (generalmente al extraer el dispositivo).
    /// </summary>
    /// <returns>True si la actualización fue exitosa</returns>
    public record UpdateActivityCmd(
        int Id,
        DateTime? ExtractedAt,
        long? FinalAvailableMB,
        long? MegabytesCopied,
        long? MegabytesDeleted,
        List<string>? FilesCopied,
        List<string>? FilesDeleted,
        string? SpecialEvent,
        ActivityStatus Status
    ) : ICommand<bool>;

    public class UpdateActivityValidator : AbstractValidator<UpdateActivityCmd>
    {
        public UpdateActivityValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Se requiere un ID de base de datos válido.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("El estado de la actividad no es válido.");

            // Si el estado es completado, la fecha de extracción es obligatoria
            RuleFor(x => x.ExtractedAt)
                .NotEmpty().WithMessage("La fecha de extracción es obligatoria cuando la actividad se completa.")
                .When(x => x.Status == ActivityStatus.Completed);

            // Si se proporciona la capacidad final, debe ser coherente
            RuleFor(x => x.FinalAvailableMB)
                .GreaterThanOrEqualTo(0).WithMessage("La capacidad disponible final debe ser un valor positivo.")
                .When(x => x.FinalAvailableMB.HasValue);

            RuleFor(x => x.MegabytesCopied)
                .GreaterThanOrEqualTo(0).WithMessage("Los MB copiados deben ser un valor positivo.")
                .When(x => x.MegabytesCopied.HasValue);

            RuleFor(x => x.MegabytesDeleted)
                .GreaterThanOrEqualTo(0).WithMessage("Los MB eliminados deben ser un valor positivo.")
                .When(x => x.MegabytesDeleted.HasValue);
        }
    }
}
