namespace DAM.Core.DTOs.Events;

/// <summary>
/// DTO simplificado para eventos del servicio.
/// </summary>
/// <remarks>
/// Versión minimalista que expone solo información esencial para clientes externos.
/// La información completa (Level, Source, Timestamp) se mantiene internamente
/// para logging y diagnóstico.
/// </remarks>
/// <param name="Id">Identificador único del evento.</param>
/// <param name="Message">Mensaje descriptivo del evento.</param>
public record ServiceEventDto(
    int Id,
    //string Level,
    //string Source,
    string Message
    //DateTime TimestampUtc
);