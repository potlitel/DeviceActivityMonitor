using DAM.Core.Entities;
using DAM.Core.Interfaces;
using DAM.Infrastructure.Audit;
using DAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DAM.Infrastructure.Repositories;

/// <summary>
/// Implementación genérica del patrón repositorio utilizando Entity Framework Core.
/// </summary>
/// <typeparam name="T">Entidad de base de datos.</typeparam>
/// <param name="db">Contexto de base de datos inyectado.</param>
public class BaseRepository<T>(DeviceActivityDbContext db) : IBaseRepository<T> where T : class
{
    /// <summary>
    /// Referencia al contexto de datos para uso en clases derivadas.
    /// </summary>
    protected readonly DeviceActivityDbContext _db = db;

    /// <inheritdoc/>
    public IQueryable<T> GetAllQueryable() => _db.Set<T>().AsNoTracking();

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _db.Set<T>().FindAsync([id], ct);

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct)
        => await _db.Set<T>().FindAsync([id], ct);
}

/// <inheritdoc cref="IActivityRepository"/>
//public class ActivityRepository(DeviceActivityDbContext db)
//    : BaseRepository<DeviceActivity>(db), IActivityRepository
//{ }

/// <inheritdoc cref="IPresenceRepository"/>
public class PresenceRepository(DeviceActivityDbContext db)
    : BaseRepository<DevicePresence>(db), IPresenceRepository
{ }

/// <inheritdoc cref="IInvoiceRepository"/>
public class InvoiceRepository(DeviceActivityDbContext db)
    : BaseRepository<Invoice>(db), IInvoiceRepository
{ }

/// <inheritdoc cref="IServiceEventRepository"/>
public class ServiceEventRepository(DeviceActivityDbContext db)
    : BaseRepository<ServiceEvent>(db), IServiceEventRepository
{ }

/// <inheritdoc cref="IAuditRepository"/>
//public class AuditRepository(DeviceActivityDbContext db)
//    : BaseRepository<AuditLog>(db), IAuditRepository
//{ }