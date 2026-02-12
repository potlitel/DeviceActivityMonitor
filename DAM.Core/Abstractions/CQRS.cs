namespace DAM.Core.Abstractions;

/// <summary>
/// 📨 Representa una **intención de cambio de estado** en el sistema.
/// </summary>
/// <typeparam name="TResponse">Tipo de dato retornado después de ejecutar el comando.</typeparam>
/// <remarks>
/// <para>
/// <b>🎯 Propósito:</b>
/// Los comandos encapsulan operaciones que **modifican el estado** del sistema.
/// Siguen el principio "Tell, Don't Ask" - indican una acción a realizar,
/// no preguntan por datos.
/// </para>
/// <para>
/// <b>📌 Características:</b>
/// <list type="bullet">
/// <item><description>✅ Representan una **acción/verbo** (Crear, Actualizar, Eliminar, Enviar)</description></item>
/// <item><description>✅ **Modifican** el estado del sistema</description></item>
/// <item><description>✅ Son **imperativos** ("Haz esto")</description></item>
/// <item><description>✅ Pueden retornar un valor (ID creado, resultado de la operación)</description></item>
/// <item><description>✅ **No** deben tener lógica de negocio - solo datos</description></item>
/// </list>
/// </para>
/// <para>
/// <b>🔒 Seguridad:</b>
/// Los comandos deben ser validados antes de su ejecución mediante FluentValidation.
/// </para>
/// </remarks>
/// <example>
/// Ejemplo de comando bien definido:
/// <code>
/// public record CreateUserCommand(
///     string Email,
///     string Password,
///     string FirstName,
///     string LastName
/// ) : ICommand&lt;Guid&gt;; // Retorna el ID del usuario creado
/// </code>
/// </example>
public interface ICommand<out TResponse> { }

/// <summary>
/// 🔍 Representa una **solicitud de información** sin efectos secundarios.
/// </summary>
/// <typeparam name="TResponse">Tipo de dato retornado por la consulta.</typeparam>
/// <remarks>
/// <para>
/// <b>🎯 Propósito:</b>
/// Las consultas separan la **lectura** de la **escritura** (CQRS).
/// Son idempotentes y no deben tener efectos secundarios observables.
/// </para>
/// <para>
/// <b>📌 Características:</b>
/// <list type="bullet">
/// <item><description>✅ Representan una **pregunta/sustantivo** (Obtener, Buscar, Listar)</description></item>
/// <item><description>✅ **No modifican** el estado del sistema</description></item>
/// <item><description>✅ Son **declarativas** ("Dame esto")</description></item>
/// <item><description>✅ Siempre retornan datos (nunca void)</description></item>
/// <item><description>✅ Pueden ser cacheadas</description></item>
/// </list>
/// </para>
/// <para>
/// <b>⚡ Optimización:</b>
/// Los handlers de consultas deben usar <c>AsNoTracking()</c> para mejorar rendimiento.
/// </para>
/// </remarks>
/// <example>
/// Ejemplo de consulta bien definida:
/// <code>
/// public record GetUserByIdQuery(Guid UserId) : IQuery&lt;UserDto&gt;;
/// </code>
/// </example>
public interface IQuery<out TResponse> { }

/// <summary>
/// 🎯 Define el **manejador** (handler) responsable de ejecutar un comando específico.
/// </summary>
/// <typeparam name="TCommand">Tipo de comando que maneja (debe implementar <see cref="ICommand{TResponse}"/>).</typeparam>
/// <typeparam name="TResponse">Tipo de respuesta del comando.</typeparam>
/// <remarks>
/// <para>
/// <b>🏗️ Responsabilidades del handler:</b>
/// <list type="number">
/// <item><description>✅ **Validar** reglas de negocio</description></item>
/// <item><description>✅ **Orquestar** repositorios y servicios de dominio</description></item>
/// <item><description>✅ **Persistir** cambios en la base de datos</description></item>
/// <item><description>✅ **Publicar** eventos de dominio (si aplica)</description></item>
/// <item><description>✅ **Manejar** transacciones</description></item>
/// </list>
/// </para>
/// <para>
/// <b>⚠️ Buenas prácticas:</b>
/// - Un handler debe manejar **solo un** tipo de comando
/// - No debe contener lógica de presentación o infraestructura
/// - Debe ser testeable unitariamente
/// </para>
/// </remarks>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Ejecuta la lógica de negocio del comando de forma asíncrona.
    /// </summary>
    /// <param name="command">Comando con los datos necesarios para la operación.</param>
    /// <param name="ct">Token de cancelación para operaciones largas.</param>
    /// <returns>Respuesta del comando (puede ser null si la operación falla).</returns>
    /// <exception cref="ValidationException">Cuando el comando no pasa las validaciones.</exception>
    /// <exception cref="DomainException">Cuando se viola una regla de negocio.</exception>
    /// <exception cref="InfrastructureException">Cuando falla la persistencia o recursos externos.</exception>
    Task<TResponse> HandleAsync(TCommand command, CancellationToken ct);
}

/// <summary>
/// 🎯 Define el **manejador** (handler) responsable de ejecutar una consulta específica.
/// </summary>
/// <typeparam name="TQuery">Tipo de consulta que maneja (debe implementar <see cref="IQuery{TResponse}"/>).</typeparam>
/// <typeparam name="TResponse">Tipo de respuesta de la consulta.</typeparam>
/// <remarks>
/// <para>
/// <b>🏗️ Responsabilidades del handler:</b>
/// <list type="number">
/// <item><description>✅ **Validar** parámetros de entrada</description></item>
/// <item><description>✅ **Optimizar** consultas (AsNoTracking, proyecciones)</description></item>
/// <item><description>✅ **Cachear** resultados (si aplica)</description></item>
/// <item><description>✅ **Proyectar** entidades a DTOs</description></item>
/// </list>
/// </para>
/// <para>
/// <b>⚡ Optimizaciones comunes:</b>
/// <code>
/// // ✅ Bueno: Proyección directa a DTO
/// return await _context.Users
///     .Where(u => u.Id == query.UserId)
///     .Select(u => new UserDto(u.Id, u.Email))
///     .FirstOrDefaultAsync(ct);
///     
/// // ❌ Malo: Cargar entidad completa y mapear manualmente
/// var user = await _context.Users.FindAsync(query.UserId);
/// return new UserDto(user.Id, user.Email);
/// </code>
/// </para>
/// </remarks>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Ejecuta la consulta y retorna los datos solicitados.
    /// </summary>
    /// <param name="query">Consulta con los criterios de búsqueda.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado de la consulta (puede ser null si no existe).</returns>
    Task<TResponse> HandleAsync(TQuery query, CancellationToken ct);
}