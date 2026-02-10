using DAM.Core.Entities;
using DAM.Infrastructure.Audit;
using DAM.Infrastructure.Persistence;
using FastEndpoints;
using System.Security.Claims;

namespace DAM.Api.Infrastructure.Auth;

/// <summary>
/// Pre-procesador global que registra cada acción realizada en la API.
/// </summary>
public class AuditPreProcessor : IGlobalPreProcessor
{
    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        // 1. Evitar auditar el propio endpoint de logs de auditoría para no crear un bucle infinito
        if (context.HttpContext.Request.Path.Value?.Contains("/audit/logs") == true) return;

        // 2. Extraer información del usuario (si está autenticado)
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        var username = context.HttpContext.User.Identity?.Name ?? "Guest";

        // 3. Crear la entrada de auditoría
        var auditEntry = new AuditLog
        {
            UserId = userId,
            Username = username,
            Action = context.HttpContext.Request.Path,
            Resource = context.HttpContext.GetEndpoint()?.DisplayName ?? "Unknown Resource",
            HttpMethod = context.HttpContext.Request.Method,
            IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0",
            TimestampUtc = DateTime.UtcNow
        };

        // 4. Persistencia (Aquí podrías inyectar el DbContext o usar un servicio)
        // Nota: Para alto rendimiento, esto suele enviarse a una cola o procesarse en background
        var db = context.HttpContext.RequestServices.GetRequiredService<DeviceActivityDbContext>();
        db.AuditLogs.Add(auditEntry);
        await db.SaveChangesAsync(ct);
    }
}