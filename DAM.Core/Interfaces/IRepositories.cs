using DAM.Core.Entities;

namespace DAM.Core.Interfaces;

/// <summary>
/// Define las operaciones de lectura base para el patrón Repositorio bajo una arquitectura desacoplada.
/// </summary>
/// <typeparam name="T">El tipo de entidad de dominio que maneja el repositorio.</typeparam>
/// <remarks>
/// <para>
/// Esta interfaz promueve el principio DRY (Don't Repeat Yourself) al centralizar el acceso a datos común.
/// El uso de <see cref="IQueryable{T}"/> permite la ejecución de filtros dinámicos y paginación 
/// directamente en el motor de base de datos (Deferred Execution).
/// </para>
/// <para>
/// <b>Inclusión de relaciones:</b> Los métodos opcionales de inclusión permiten cargar relaciones
/// de manera explícita y controlada, evitando el problema N+1 y manteniendo la intencionalidad clara.
/// </para>
/// </remarks>
public interface IBaseRepository<T> where T : class
{
    /// <summary>
    /// Expone la colección de entidades como una consulta evaluable.
    /// </summary>
    /// <param name="includes">Expresiones para incluir entidades relacionadas (opcional).</param>
    /// <returns>Un objeto <see cref="IQueryable{T}"/> configurado para lecturas optimizadas.</returns>
    /// <remarks>
    /// Utilizar <c>AsNoTracking()</c> por defecto para mejorar rendimiento en operaciones de solo lectura.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Obtener queryable sin relaciones
    /// var query = repository.GetAllQueryable();
    /// 
    /// // Obtener queryable incluyendo relaciones
    /// var queryWithIncludes = repository.GetAllQueryable(
    ///     x => x.Invoices,
    ///     x => x.Activities
    /// );
    /// </code>
    /// </example>
    IQueryable<T> GetAllQueryable(params Func<IQueryable<T>, IQueryable<T>>[] includes);

    /// <summary>
    /// Recupera una entidad específica mediante su identificador único de forma asíncrona.
    /// </summary>
    /// <typeparam name="TId">Tipo del identificador (Guid, int, string, etc.).</typeparam>
    /// <param name="id">Identificador único de la entidad.</param>
    /// <param name="ct">Token de cancelación para abortar la operación si es necesario.</param>
    /// <param name="includes">Expresiones para incluir entidades relacionadas (opcional).</param>
    /// <returns>La entidad encontrada o <see langword="null"/> si no existe coincidencia.</returns>
    /// <remarks>
    /// <para>
    /// Método genérico que soporta cualquier tipo de identificador mediante pattern matching.
    /// </para>
    /// <para>
    /// <b>Seguridad:</b> Validar siempre el tipo de identificador en implementaciones concretas
    /// para prevenir ataques de inyección de tipos.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Obtener por GUID sin relaciones
    /// var entity = await repository.GetByIdAsync(Guid.NewGuid(), ct);
    /// 
    /// // Obtener por int con relaciones
    /// var entityWithIncludes = await repository.GetByIdAsync(
    ///     1, 
    ///     ct,
    ///     query => query.Include(x => x.Invoices)
    ///                    .Include(x => x.Activities)
    /// );
    /// </code>
    /// </example>
    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken ct, params Func<IQueryable<T>, IQueryable<T>>[] includes);
}

/// <summary>
/// Repositorio especializado para el historial de presencia y conexión física de hardware.
/// </summary>
public interface IPresenceRepository : IBaseRepository<DevicePresence> { }

/// <summary>
/// Repositorio especializado para el control y persistencia de facturación procesada.
/// </summary>
public interface IInvoiceRepository : IBaseRepository<Invoice> { }

/// <summary>
/// Repositorio especializado para el registro de la "Caja Negra" y eventos de diagnóstico del sistema.
/// </summary>
public interface IServiceEventRepository : IBaseRepository<ServiceEvent> { }

/// <summary>
/// Repositorio especializado para la trazabilidad de acciones de usuario y auditoría de seguridad.
/// </summary>
public interface IAuditRepository : IBaseRepository<AuditLog> { }