using DAM.Core.Entities;
using DAM.Infrastructure.Persistence;

namespace DAM.Api.Infrastructure.Auth
{
    /// <summary>
    /// Implementación base del servicio de auditoría utilizando Entity Framework Core.
    /// </summary>
    /// <param name="db">Contexto de base de datos inyectado.</param>
    public class AuditService(DeviceActivityDbContext db) : IAuditService
    {
        /// <inheritdoc />
        /// <remarks>
        /// La persistencia se realiza de forma atómica en la tabla 'AuditLogs'. 
        /// En escenarios de alta carga, se recomienda sustituir esta implementación por una basada en colas (Message Broker).
        /// </remarks>
        public async Task LogAsync(AuditLog entry, CancellationToken ct = default)
        {
            try
            {
                db.AuditLogs.Add(entry);
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                // 💡 "Wrap and Throw": Envolvemos la excepción original para dar contexto
                throw new InfrastructureException("Error crítico al persistir el log de auditoría.", ex);
            }
        }
    }
}
