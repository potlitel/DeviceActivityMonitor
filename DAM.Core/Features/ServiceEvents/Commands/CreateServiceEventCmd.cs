using DAM.Core.Abstractions;
using FluentValidation;

namespace DAM.Core.Features.ServiceEvents.Commands
{
    /// <summary>
    /// Comando para registrar eventos internos del servicio de Windows.
    /// </summary>
    /// <returns>El ID del evento de servicio creado</returns>
    public record CreateServiceEventCmd(
        DateTime Timestamp,
        string EventType,
        string Message
    ) : ICommand<int>;

    public class CreateServiceEventValidator : AbstractValidator<CreateServiceEventCmd>
    {
        public CreateServiceEventValidator()
        {
            RuleFor(x => x.Timestamp)
                .NotEmpty().WithMessage("La fecha y hora del evento es obligatoria.");

            RuleFor(x => x.EventType)
                .NotEmpty().WithMessage("El tipo de evento es obligatorio.")
                .MaximumLength(50).WithMessage("El tipo de evento no puede exceder los 50 caracteres.");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("El mensaje del evento es obligatorio.");
        }
    }
}
