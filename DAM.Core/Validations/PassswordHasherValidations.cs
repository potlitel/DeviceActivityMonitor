using FluentValidation;

namespace DAM.Core.Validations
{
    /// <summary>
    /// Validador FluentValidation para contraseñas en texto plano.
    /// </summary>
    public class PasswordValidator : AbstractValidator<string>
    {
        public PasswordValidator()
        {
            RuleFor(password => password)
                .NotNull().WithMessage("La contraseña no puede ser nula.")
                .NotEmpty().WithMessage("La contraseña no puede estar vacía.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .MaximumLength(128).WithMessage("La contraseña no puede exceder los 128 caracteres.");
        }
    }

    /// <summary>
    /// Validador FluentValidation para hash de BCrypt.
    /// </summary>
    public class BcryptHashValidator : AbstractValidator<string>
    {
        public BcryptHashValidator()
        {
            RuleFor(hash => hash)
                .NotNull().WithMessage("El hash no puede ser nulo.")
                .NotEmpty().WithMessage("El hash no puede estar vacío.")
                .Must(BeValidBcryptFormat).WithMessage("El hash no tiene un formato BCrypt válido.");
        }

        private bool BeValidBcryptFormat(string hash)
        {
            // Formato BCrypt: $2a$12$salt.hash (60 caracteres)
            return !string.IsNullOrEmpty(hash) &&
                   hash.Length == 60 &&
                   hash.StartsWith("$2");
        }
    }

    /// <summary>
    /// Validador compuesto que reutiliza reglas existentes.
    /// </summary>
    public class StrongPasswordValidator : AbstractValidator<string>
    {
        public StrongPasswordValidator()
        {
            // Reutiliza todas las validaciones base
            Include(new PasswordValidator());

            // Añade reglas adicionales de complejidad
            RuleFor(password => password)
                .Matches(@"[A-Z]").WithMessage("Debe contener al menos una mayúscula.")
                .Matches(@"[a-z]").WithMessage("Debe contener al menos una minúscula.")
                .Matches(@"[0-9]").WithMessage("Debe contener al menos un número.")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("Debe contener al menos un carácter especial.");
        }
    }

    /// <summary>
    /// Configuración tipada para BCrypt con validación incorporada.
    /// </summary>
    public class BCryptSettings
    {
        public const int DefaultWorkFactor = 12;
        public const bool DefaultEnforceStrongPasswords = true;

        /// <summary>
        /// Factor de trabajo (costo). Rango recomendado: 10-14.
        /// </summary>
        public int WorkFactor { get; set; } = DefaultWorkFactor;

        /// <summary>
        /// Indica si se debe exigir contraseñas fuertes (mayúsculas, minúsculas, números, especiales).
        /// </summary>
        public bool EnforceStrongPasswords { get; set; } = DefaultEnforceStrongPasswords;

        /// <summary>
        /// Versión del algoritmo BCrypt ($2a, $2b, $2y).
        /// </summary>
        public string AlgorithmVersion { get; set; } = "$2a";
    }

    /// <summary>
    /// Validador para la configuración de BCrypt.
    /// </summary>
    public class BCryptSettingsValidator : AbstractValidator<BCryptSettings>
    {
        public BCryptSettingsValidator()
        {
            RuleFor(x => x.WorkFactor)
                .InclusiveBetween(4, 31)
                .WithMessage("WorkFactor debe estar entre 4 y 31");

            RuleFor(x => x.AlgorithmVersion)
                .Must(v => new[] { "$2a", "$2b", "$2y" }.Contains(v))
                .WithMessage("Versión de algoritmo inválida");
        }
    }
}
