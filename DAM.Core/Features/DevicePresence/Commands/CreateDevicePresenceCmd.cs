using DAM.Core.Abstractions;
using FluentValidation;

namespace DAM.Core.Features.DevicePresence.Commands
{
    /// <summary>
    /// Comando para registrar la presencia puntual de un dispositivo.
    /// </summary>
    /// <returns>El ID del registro de presencia creado</returns>
    public record CreateDevicePresenceCmd(
        string SerialNumber,
        DateTime Timestamp,
        int DeviceActivityId
    ) : ICommand<int>;

    public class CreateDevicePresenceValidator : AbstractValidator<CreateDevicePresenceCmd>
    {
        public CreateDevicePresenceValidator()
        {
            RuleFor(x => x.SerialNumber)
                .NotEmpty().WithMessage("El Serial Number es obligatorio.");

            RuleFor(x => x.Timestamp)
                .NotEmpty().WithMessage("La fecha y hora de presencia es obligatoria.");

            RuleFor(x => x.DeviceActivityId)
                .GreaterThan(0).WithMessage("Se requiere un ID de actividad de dispositivo válido.");
        }
    }
}
