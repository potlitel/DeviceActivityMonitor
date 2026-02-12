using DAM.Core.Entities;
using DAM.Core.Interfaces;
using DAM.Infrastructure.Audit;
using DAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DAM.Infrastructure.Repositories;

/// <summary>
/// Implementación genérica del patrón repositorio utilizando Entity Framework Core.
/// </summary>
/// <typeparam name="T">Entidad de base de datos.</typeparam>
/// <param name="db">Contexto de base de datos inyectado.</param>
/// <remarks>
/// <para>
/// Proporciona una implementación base que puede ser extendida por repositorios específicos
/// de cada entidad. Incluye soporte para carga explícita de relaciones (Eager Loading).
/// </para>
/// <para>
/// <b>Principios de diseño:</b>
/// <list type="bullet">
/// <item><description><b>Open/Closed:</b> Extensible para necesidades específicas</description></item>
/// <item><description><b>Interface Segregation:</b> Métodos separados por responsabilidad</description></item>
/// <item><description><b>Liskov Substitution:</b> Repositorios específicos pueden reemplazar esta implementación</description></item>
/// </list>
/// </para>
/// </remarks>
public class BaseRepository<T>(DeviceActivityDbContext db) : IBaseRepository<T> where T : class
{
    /// <summary>
    /// Referencia al contexto de datos para uso en clases derivadas.
    /// </summary>
    protected readonly DeviceActivityDbContext _db = db;

    /// <summary>
    /// Referencia al DbSet de la entidad para operaciones específicas.
    /// </summary>
    protected readonly DbSet<T> _set = db.Set<T>();

    /// <inheritdoc/>
    public IQueryable<T> GetAllQueryable(params Func<IQueryable<T>, IQueryable<T>>[] includes)
    {
        IQueryable<T> query = _set.AsNoTracking();

        // Aplicar includes si se proporcionaron
        foreach (var include in includes)
        {
            query = include(query);
        }

        return query;
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken ct, params Func<IQueryable<T>, IQueryable<T>>[] includes)
    {
        IQueryable<T> query = _set.AsQueryable();

        // Aplicar includes si se proporcionaron
        foreach (var include in includes)
        {
            query = include(query);
        }

        // Manejar diferentes tipos de identificadores
        return id switch
        {
            Guid guidId => await query.FirstOrDefaultAsync(GenerateGuidPredicate(guidId), ct),
            int intId => await query.FirstOrDefaultAsync(GenerateIntPredicate(intId), ct),
            string stringId => await query.FirstOrDefaultAsync(GenerateStringPredicate(stringId), ct),
            _ => throw new NotSupportedException($"Tipo de identificador {typeof(TId)} no soportado")
        };
    }

    /// <summary>
    /// Genera una expresión para filtrar por identificador GUID.
    /// </summary>
    /// <remarks>
    /// Asume que la entidad tiene una propiedad "Id" de tipo GUID.
    /// Repositorios específicos pueden sobreescribir este método para lógicas personalizadas.
    /// </remarks>
    protected virtual Expression<Func<T, bool>> GenerateGuidPredicate(Guid id)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, "Id");
        var constant = Expression.Constant(id);
        var equality = Expression.Equal(property, constant);

        return Expression.Lambda<Func<T, bool>>(equality, parameter);
    }

    /// <summary>
    /// Genera una expresión para filtrar por identificador entero.
    /// </summary>
    protected virtual Expression<Func<T, bool>> GenerateIntPredicate(int id)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, "Id");
        var constant = Expression.Constant(id);
        var equality = Expression.Equal(property, constant);

        return Expression.Lambda<Func<T, bool>>(equality, parameter);
    }

    /// <summary>
    /// Genera una expresión para filtrar por identificador string.
    /// </summary>
    protected virtual Expression<Func<T, bool>> GenerateStringPredicate(string id)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, "Id");
        var constant = Expression.Constant(id);
        var equality = Expression.Equal(property, constant);

        return Expression.Lambda<Func<T, bool>>(equality, parameter);
    }
}

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
public class AuditRepository(DeviceActivityDbContext db)
    : BaseRepository<AuditLog>(db), IAuditRepository
{ }

/// <summary>
/// Extensiones para facilitar el uso de includes con expresiones type-safe.
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Crea una función de inclusión type-safe para usar en los métodos del repositorio.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad principal.</typeparam>
    /// <typeparam name="TProperty">Tipo de la propiedad de navegación.</typeparam>
    /// <param name="includeExpression">Expresión que especifica la propiedad a incluir.</param>
    /// <returns>Función que puede pasarse a los métodos del repositorio.</returns>
    /// <example>
    /// <code>
    /// // Crear función de inclusión
    /// var includeFunc = RepositoryExtensions.Include&lt;Device, Invoice&gt;(x => x.Invoices);
    /// 
    /// // Usar en el repositorio
    /// var device = await repository.GetByIdAsync(
    ///     deviceId, 
    ///     ct, 
    ///     includeFunc
    /// );
    /// </code>
    /// </example>
    public static Func<IQueryable<T>, IQueryable<T>> Include<T, TProperty>(
        Expression<Func<T, TProperty>> includeExpression)
        where T : class
    {
        return query => query.Include(includeExpression);
    }

    /// <summary>
    /// Crea una función para inclusión con ThenInclude.
    /// </summary>
    public static Func<IQueryable<T>, IQueryable<T>> ThenInclude<T, TPreviousProperty, TProperty>(
        Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression)
        where T : class
    {
        return query => ((IIncludableQueryable<T, TPreviousProperty>)query).ThenInclude(thenIncludeExpression);
    }
}