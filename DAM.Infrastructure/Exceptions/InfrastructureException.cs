/// <summary>
/// Excepción base para errores ocurridos en la capa de persistencia o servicios externos.
/// </summary>
/// <remarks>
/// Se utiliza para encapsular excepciones técnicas (SQL, IO, Red) y evitar que detalles 
/// sensibles de la infraestructura se filtren a las capas superiores.
/// </remarks>
public class InfrastructureException : Exception
{
    public InfrastructureException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Excepción de dominio lanzada cuando se viola la regla de unicidad de identidad de usuario.
/// </summary>
/// <remarks>
/// Esta excepción debe ser capturada por la capa API para retornar un conflicto (HTTP 409).
/// </remarks>
public class DuplicateEmailException : Exception
{
    /// <summary> Inicializa una nueva instancia con un mensaje específico y la excepción raíz. </summary>
    public DuplicateEmailException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary> Inicializa una nueva instancia con un mensaje descriptivo. </summary>
    public DuplicateEmailException(string message) : base(message) { }
}