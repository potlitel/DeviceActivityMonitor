using DAM.Core.Entities;

namespace DAM.Api.Infrastructure.Auth
{
    /// <summary>
    /// Define el contrato para el servicio de auditoría del sistema.
    /// </summary>
    /// <remarks>
    /// Este servicio es responsable de centralizar el registro de actividades de usuario,
    /// permitiendo cumplir con normativas de seguridad y trazabilidad de operaciones (Audit Trail).
    /// </remarks>
    public interface IAuditService
    {
        /// <summary>
        /// Registra de forma persistente una entrada de auditoría.
        /// </summary>
        /// <param name="entry">Objeto que contiene los metadatos de la acción realizada.</param>
        /// <param name="ct">Token de cancelación para operaciones asíncronas.</param>
        /// <returns>Una tarea que representa la operación de guardado asíncrono.</returns>
        Task LogAsync(AuditLog entry, CancellationToken ct = default);
    }
}
