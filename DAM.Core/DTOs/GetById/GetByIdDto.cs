using FluentValidation;

/// <summary>
/// Solicitud para obtener un recurso por su identificador GUID.
/// </summary>
/// <param name="Id">Identificador único global (GUID) del recurso.</param>
/// <remarks>
/// Utilizado principalmente para entidades con claves de tipo GUID.
/// </remarks>
public record GetByIdRequest(Guid Id);

/// <summary>
/// Solicitud para obtener un recurso por su identificador entero.
/// </summary>
/// <param name="Id">Identificador entero del recurso.</param>
/// <remarks>
/// Utilizado para entidades con claves autoincrementales o de tipo INT.
/// </remarks>
public record GetByIdIntRequest(int Id);

/// <summary>
/// Validador para solicitudes con identificador GUID.
/// </summary>
public class GetByIdValidator : AbstractValidator<GetByIdRequest>
{
    public GetByIdValidator() => RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador GUID es obligatorio.");
}

/// <summary>
/// Validador para solicitudes con identificador entero.
/// </summary>
public class GetByIdIntValidator : AbstractValidator<GetByIdIntRequest>
{
    public GetByIdIntValidator() => RuleFor(x => x.Id).GreaterThan(0).WithMessage("El identificador debe ser un entero positivo.");
}