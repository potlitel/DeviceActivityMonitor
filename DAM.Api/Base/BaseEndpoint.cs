using FastEndpoints;
using DAM.Core.Common;

namespace DAM.Api.Base;

/// <summary>
/// Clase base para todos los endpoints de la API que asegura una estructura de respuesta universal.
/// </summary>
/// <typeparam name="TRequest">Tipo de la petición de entrada.</typeparam>
/// <typeparam name="TResponse">Tipo del objeto de datos de salida.</typeparam>
public abstract class BaseEndpoint<TRequest, TResponse> : Endpoint<TRequest, ApiResponse<TResponse>>
    where TRequest : notnull
{
    /// <summary>
    /// Envía una respuesta exitosa (HTTP 200) envuelta en <see cref="ApiResponse{T}"/>.
    /// </summary>
    protected async Task SendSuccessAsync(TResponse data, string message = "Success", CancellationToken ct = default)
    {
        await Send.OkAsync(ApiResponse<TResponse>.Ok(data, message), ct);
    }

    /// <summary>
    /// Envía una respuesta de error estandarizada, capturando los fallos de validación o errores personalizados.
    /// </summary>
    /// <param name="statusCode">Código de estado HTTP (ej. 400, 401, 404).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <remarks>
    /// Este método extrae automáticamente los errores de la colección 'ValidationFailures' de FastEndpoints
    /// y los encapsula en la lista de errores de nuestra respuesta universal.
    /// </remarks>
    protected async Task SendErrorsAsync(int statusCode = 400, CancellationToken ct = default)
    {
        // Extraemos los mensajes de error de la validación de FastEndpoints/FluentValidation
        var errorList = ValidationFailures
            .Select(f => f.ErrorMessage)
            .ToList();

        // Si no hay errores de validación pero se llamó al método, añadimos un mensaje genérico
        if (errorList.Count == 0)
            errorList.Add("Se ha producido un error inesperado en la solicitud.");

        var response = ApiResponse<TResponse>.Failure(errorList, "Error de validación o solicitud");

        // Convertimos nuestro ApiResponse en un IResult compatible
        var result = Results.Json(response, statusCode: statusCode);
        await Send.ResultAsync(result);
    }
}