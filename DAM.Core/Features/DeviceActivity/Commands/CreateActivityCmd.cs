using DAM.Core.Abstractions;
using DAM.Core.Enums;
using FluentValidation;

namespace DAM.Core.Features.DeviceActivity.Commands
{
    /// <summary>
    /// Comando para la creación de una nueva actividad de dispositivo.
    /// </summary>
    public record CreateActivityCmd(
        string SerialNumber,
        string Model,
        long TotalCapacityMB,
        DateTime InsertedAt,
        long InitialAvailableMB,
        ActivityStatus Status
    ) : ICommand<int>;

    public class CreateActivityValidator : AbstractValidator<CreateActivityCmd>
    {
        public CreateActivityValidator()
        {
            RuleFor(x => x.SerialNumber)
                .NotEmpty().WithMessage("El Serial Number es obligatorio.");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("El Modelo es obligatorio.");

            RuleFor(x => x.TotalCapacityMB)
                .GreaterThan(0).WithMessage("La capacidad total debe ser mayor a 0.");

            RuleFor(x => x.InsertedAt)
                .NotEmpty().WithMessage("La fecha de inserción es obligatoria.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("La fecha de inserción no puede ser futura.");

            RuleFor(x => x.InitialAvailableMB)
                .GreaterThanOrEqualTo(0).WithMessage("La capacidad disponible inicial debe ser un valor positivo.")
                .LessThanOrEqualTo(x => x.TotalCapacityMB).WithMessage("La capacidad disponible inicial no puede exceder la capacidad total del dispositivo.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("El estado de la actividad no es válido.");
        }
    }

}
