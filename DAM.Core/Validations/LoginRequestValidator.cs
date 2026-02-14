using DAM.Core.DTOs.Login;
using FluentValidation;

namespace DAM.Core.Validations
{
    /// <summary>
    /// Reglas de validación para el proceso de inicio de sesión.
    /// </summary>
    /// <remarks>
    /// Valida la estructura del email y la complejidad mínima de la contraseña 
    /// para reducir la carga de procesamiento en el IdentityService.
    /// </remarks>
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");
        }
    }
}
