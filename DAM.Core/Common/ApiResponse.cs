namespace DAM.Core.Common;

/// <summary>
/// Estructura universal para todas las respuestas de la API.
/// </summary>
/// <typeparam name="T">Tipo de dato que contiene la respuesta.</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Success")
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Failure(List<string> errors, string message = "Error")
        => new() { Success = false, Errors = errors, Message = message };
}