namespace DAM.Core.DTOs.AuditLogs
{
    /// <summary>
    /// Objeto de transferencia de datos para los registros de auditoría del sistema.
    /// </summary>
    /// <param name="Id">Identificador único del registro.</param>
    /// <param name="UserId">Identidad del usuario que realizó la acción.</param>
    /// <param name="Action">Ruta del endpoint o acción ejecutada.</param>
    /// <param name="Resource">Nombre del recurso afectado (ej: "DeviceActivity").</param>
    /// <param name="HttpMethod">Método HTTP utilizado (GET, POST, etc.).</param>
    /// <param name="TimestampUtc">Fecha y hora de la operación en formato UTC.</param>
    public record AuditLogDto(
        Guid Id,
        string UserId,
        string Action,
        string Resource,
        string HttpMethod,
        DateTime TimestampUtc
    );
}
