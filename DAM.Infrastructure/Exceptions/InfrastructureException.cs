/// <summary>
/// Excepción lanzada cuando ocurre un error en la capa de infraestructura.
/// </summary>
public class InfrastructureException : Exception
{
    public InfrastructureException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Excepción lanzada cuando se intenta registrar un email ya existente.
/// </summary>
public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string message, Exception innerException)
        : base(message, innerException) { }

    public DuplicateEmailException(string message) : base(message) { }
}