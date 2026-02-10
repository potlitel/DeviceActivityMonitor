using DAM.Core.Entities;

namespace DAM.Core.Interfaces;

/// <summary>
/// Define las operaciones de lectura base para el patrón Repositorio bajo una arquitectura desacoplada.
/// </summary>
/// <typeparam name="T">El tipo de entidad de dominio que maneja el repositorio.</typeparam>
/// <remarks>
/// Esta interfaz promueve el principio DRY (Don't Repeat Yourself) al centralizar el acceso a datos común.
/// El uso de <see cref="IQueryable{T}"/> permite la ejecución de filtros dinámicos y paginación 
/// directamente en el motor de base de datos (Deferred Execution).
/// </remarks>
public interface IBaseRepository<T> where T : class
{
    /// <summary>
    /// Expone la colección de entidades como una consulta evaluable.
    /// </summary>
    /// <returns>Un objeto <see cref="IQueryable{T}"/> configurado para lecturas optimizadas.</returns>
    IQueryable<T> GetAllQueryable();

    /// <summary>
    /// Recupera una entidad específica mediante su identificador único de forma asíncrona.
    /// </summary>
    /// <param name="id">Identificador único global (GUID) de la entidad.</param>
    /// <param name="ct">Token de cancelación para abortar la operación si es necesario.</param>
    /// <returns>La entidad encontrada o <see langword="null"/> si no existe coincidencia.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Recupera una entidad específica mediante su identificador único de forma asíncrona.
    /// </summary>
    /// <param name="id">Identificador único global (GUID) de la entidad.</param>
    /// <param name="ct">Token de cancelación para abortar la operación si es necesario.</param>
    /// <returns>La entidad encontrada o <see langword="null"/> si no existe coincidencia.</returns>
    Task<T?> GetByIdAsync(int id, CancellationToken ct);
}

/// <summary>
/// Repositorio especializado para la gestión de actividades de dispositivos.
/// </summary>
//public interface IActivityRepository : IBaseRepository<DeviceActivity> { }

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
//public interface IAuditRepository : IBaseRepository<AuditLog> { }