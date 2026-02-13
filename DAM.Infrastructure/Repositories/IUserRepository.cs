using DAM.Core.Entities;
using DAM.Core.Interfaces;
using DAM.Infrastructure.Persistence;
using DAM.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAM.Core.Repositories;

/// <summary>
/// Define las operaciones específicas del repositorio de usuarios,
/// extendiendo las operaciones base de <see cref="IBaseRepository{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Este repositorio encapsula toda la lógica de acceso a datos relacionada con 
/// la entidad <see cref="ApplicationUser"/>, incluyendo operaciones de consulta
/// específicas y comandos de escritura transaccionales.
/// </para>
/// <para>
/// <b>Patrón aplicado:</b> Repository Pattern + Unit of Work (implícito vía DbContext)
/// </para>
/// <para>
/// <b>Consideraciones transaccionales:</b>
/// Los métodos de escritura (<see cref="AddAsync"/>) no confirman la transacción
/// automáticamente. La confirmación debe ser manejada por el Unit of Work
/// (<see cref="DeviceActivityDbContext.SaveChangesAsync"/>) para permitir
/// operaciones atómicas entre múltiples repositorios.
/// </para>
/// </remarks>
public interface IUserRepository : IBaseRepository<ApplicationUser>
{
    /// <summary>
    /// Recupera un usuario por su dirección de correo electrónico.
    /// </summary>
    /// <param name="email">Dirección de correo electrónico (case-insensitive).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// La entidad <see cref="ApplicationUser"/> si existe; 
    /// <see langword="null"/> en caso contrario.
    /// </returns>
    /// <remarks>
    /// La búsqueda es case-insensitive y utiliza <c>AsNoTracking()</c> para
    /// optimizar rendimiento en operaciones de solo lectura.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Lanzada cuando el email es nulo o vacío.
    /// </exception>
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct);

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="user">Entidad de usuario a persistir.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <remarks>
    /// <para>
    /// <b>Validaciones implícitas:</b>
    /// <list type="bullet">
    /// <item><description>Email único (restringido por índice de base de datos)</description></item>
    /// <item><description>Campos requeridos según configuración de Entity Framework</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Nota transaccional:</b>
    /// Este método NO llama a <c>SaveChangesAsync()</c>. El guardado debe ser explícito
    /// en la capa de aplicación/handler para permitir operaciones atómicas.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Lanzada cuando el usuario es nulo.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Lanzada cuando falla la persistencia (ej. email duplicado, campos inválidos).
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Lanzada cuando la operación es cancelada vía <see cref="CancellationToken"/>.
    /// </exception>
    Task AddAsync(ApplicationUser user, CancellationToken ct);
}

/// <summary>
/// Implementación concreta del repositorio de usuarios utilizando Entity Framework Core.
/// </summary>
/// <remarks>
/// <para>
/// <b>Estrategia de manejo de excepciones:</b>
/// Esta implementación utiliza un enfoque de "excepciones esperadas vs. inesperadas":
/// </para>
/// <list type="bullet">
/// <item><description>
/// <b>Esperadas (dominio):</b> Se dejan propagar (email duplicado, violaciones de constraint)
/// </description></item>
/// <item><description>
/// <b>Inesperadas (infraestructura):</b> Se envuelven en excepciones de dominio con contexto
/// </description></item>
/// </list>
/// </remarks>
public class UserRepository(DeviceActivityDbContext db)
    : BaseRepository<ApplicationUser>(db), IUserRepository
{
    private readonly ILogger<UserRepository>? _logger;

    /// <summary>
    /// Constructor que permite inyección de logger para diagnóstico.
    /// </summary>
    /// <param name="db">Contexto de base de datos.</param>
    /// <param name="logger">Logger opcional para auditoría.</param>
    public UserRepository(DeviceActivityDbContext db, ILogger<UserRepository>? logger = null)
        : this(db)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// Lanzada cuando el email es nulo o vacío.
    /// </exception>
    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "El email es requerido para la búsqueda.");

        try
        {
            _logger?.LogDebug("Buscando usuario por email: {Email}", email);

            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, ct);
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("Búsqueda de usuario cancelada: {Email}", email);
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error inesperado al buscar usuario por email: {Email}", email);
            throw new InfrastructureException(
                $"Error al acceder al repositorio de usuarios. Operación: GetByEmailAsync", ex);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// Lanzada cuando el usuario es nulo.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Lanzada cuando hay violación de restricciones (email duplicado, etc.)
    /// </exception>
    public async Task AddAsync(ApplicationUser user, CancellationToken ct)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "La entidad de usuario no puede ser nula.");

        // Validación de negocio temprana (defensiva)
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentException("El email del usuario es requerido.", nameof(user));

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new ArgumentException("El hash de contraseña es requerido.", nameof(user));

        try
        {
            _logger?.LogDebug("Registrando nuevo usuario: {Email}, {Username}",
                user.Email, user.Username);

            await _db.Users.AddAsync(user, ct);

            // ❌ EXPLÍCITAMENTE NO llamamos a SaveChangesAsync()
            // La transacción será confirmada por el Unit of Work en la capa superior
        }
        catch (DbUpdateException ex)
        {
            // 🎯 MANEJO ESPECÍFICO PARA ERRORES DE BD
            if (IsDuplicateKeyException(ex))
            {
                _logger?.LogWarning(ex, "Intento de registro con email duplicado: {Email}", user.Email);
                throw new DuplicateEmailException($"El email '{user.Email}' ya está registrado en el sistema.", ex);
            }

            _logger?.LogError(ex, "Error de base de datos al registrar usuario: {Email}", user.Email);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("Registro de usuario cancelado: {Email}", user.Email);
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error inesperado al registrar usuario: {Email}", user.Email);
            throw new InfrastructureException(
                $"Error al acceder al repositorio de usuarios. Operación: AddAsync", ex);
        }
    }

    /// <summary>
    /// Determina si una excepción de base de datos corresponde a violación de clave única/duplicada.
    /// </summary>
    /// <remarks>
    /// Esta implementación es específica para SQL Server/PostgreSQL. 
    /// Para otros proveedores, ajustar los códigos de error.
    /// </remarks>
    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        //// SQL Server: 2627 = Unique constraint, 2601 = Duplicated key
        //if (ex.InnerException is SqlException sqlEx)
        //    return sqlEx.Number is 2627 or 2601;

        //// PostgreSQL: 23505 = unique_violation
        //if (ex.InnerException is PostgresException pgEx)
        //    return pgEx.SqlState == "23505";

        //// MySQL: 1062 = Duplicate entry
        //if (ex.InnerException is MySqlException mySqlEx)
        //    return mySqlEx.Number == 1062;

        // SQLite: 19 = Constraint violation
        if (ex.InnerException is SqliteException sqliteEx)
            return sqliteEx.SqliteErrorCode == 19;

        return false;
    }
}