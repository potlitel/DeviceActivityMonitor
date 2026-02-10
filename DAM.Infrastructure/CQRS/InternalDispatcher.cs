using Microsoft.Extensions.DependencyInjection;
using DAM.Core.Abstractions;

namespace DAM.Infrastructure.CQRS;

/// <summary>
/// Define el orquestador central para el patrón Mediator, actuando como bus de mensajes interno.
/// </summary>
/// <remarks>
/// <para>
/// Este componente desacopla completamente la emisión de comandos/consultas de su ejecución lógica,
/// facilitando la implementación de Cross-Cutting Concerns como logging, validación, auditoría y manejo de excepciones.
/// </para>
/// <para>
/// Patrones aplicados:
/// <list type="bullet">
/// <item><description><b>Mediator</b>: Centraliza la comunicación entre objetos</description></item>
/// <item><description><b>Command</b>: Encapsula solicitudes como objetos</description></item>
/// <item><description><b>CQRS</b>: Separación clara entre comandos (escritura) y consultas (lectura)</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Beneficios:</b>
/// <list type="number">
/// <item><description>Reducción del acoplamiento entre componentes</description></item>
/// <item><description>Mantenimiento centralizado de preocupaciones transversales</description></item>
/// <item><description>Mayor testabilidad mediante mocks del dispatcher</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Uso típico en una API Controller:
/// <code>
/// public class ActivitiesController : ControllerBase
/// {
///     private readonly IDispatcher _dispatcher;
///     
///     public ActivitiesController(IDispatcher dispatcher)
///     {
///         _dispatcher = dispatcher;
///     }
///     
///     [HttpGet]
///     public async Task&lt;IActionResult&gt; GetActivities([FromQuery] ActivityFilter filter)
///     {
///         var query = new GetActivitiesQuery(filter);
///         var result = await _dispatcher.QueryAsync(query);
///         return Ok(result);
///     }
///     
///     [HttpPost]
///     public async Task&lt;IActionResult&gt; CreateActivity([FromBody] CreateActivityCommand command)
///     {
///         var result = await _dispatcher.SendAsync(command);
///         return CreatedAtAction(nameof(GetActivity), new { id = result.Id }, result);
///     }
/// }
/// </code>
/// </example>
public interface IDispatcher
{
    /// <summary>
    /// Envía un comando que modifica el estado del sistema y retorna un resultado.
    /// </summary>
    /// <typeparam name="TResponse">Tipo de respuesta esperada.</typeparam>
    /// <param name="command">Comando a ejecutar, implementando <see cref="ICommand{TResponse}"/>.</param>
    /// <param name="ct">Token de cancelación para operaciones asíncronas.</param>
    /// <returns>Respuesta generada por el handler del comando.</returns>
    /// <exception cref="HandlerNotFoundException">Cuando no se encuentra un handler registrado para el comando.</exception>
    /// <remarks>
    /// Los comandos siguen el principio de "Tell, Don't Ask" - indican una acción a realizar,
    /// no preguntan por datos. Suelen modificar el estado del sistema y pueden generar eventos de dominio.
    /// </remarks>
    /// <example>
    /// <code>
    /// var command = new CreateInvoiceCommand(serialNumber: "DEV-001", amount: 99.99m);
    /// var invoiceId = await dispatcher.SendAsync&lt;Guid&gt;(command);
    /// </code>
    /// </example>
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct = default);

    /// <summary>
    /// Envía una consulta que recupera datos del sistema sin modificar su estado.
    /// </summary>
    /// <typeparam name="TResponse">Tipo de la respuesta esperada.</typeparam>
    /// <param name="query">Consulta a ejecutar, implementando <see cref="IQuery{TResponse}"/>.</param>
    /// <param name="ct">Token de cancelación para operaciones asíncronas.</param>
    /// <returns>Resultado de la consulta.</returns>
    /// <exception cref="HandlerNotFoundException">Cuando no se encuentra un handler registrado para la consulta.</exception>
    /// <remarks>
    /// Las consultas son idempotentes y no deben tener efectos secundarios. Se recomienda utilizar
    /// <see cref="AsNoTracking"/> en Entity Framework para optimizar el rendimiento.
    /// </remarks>
    /// <example>
    /// <code>
    /// var query = new GetUserProfileQuery(userId: Guid.NewGuid());
    /// var profile = await dispatcher.QueryAsync&lt;ProfileResponse&gt;(query);
    /// </code>
    /// </example>
    Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct = default);
}

/// <summary>
/// Implementación concreta del dispatcher que localiza y ejecuta Handlers dinámicamente mediante Reflection.
/// </summary>
/// <remarks>
/// <para>
/// Este componente actúa como el núcleo del patrón Mediator, resolviendo las dependencias de handlers
/// a través del contenedor de DI y ejecutándolos de forma transparente.
/// </para>
/// <para>
/// <b>Flujo de ejecución:</b>
/// <list type="number">
/// <item><description>Recibe un comando/consulta</description></item>
/// <item><description>Determina el tipo de handler requerido mediante Reflection</description></item>
/// <item><description>Resuelve el handler del contenedor de DI</description></item>
/// <item><description>Invoca el método HandleAsync del handler</description></item>
/// <item><description>Retorna el resultado al caller</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Consideraciones de rendimiento:</b>
/// <list type="bullet">
/// <item><description>El uso de Reflection tiene un costo mínimo que se amortiza en aplicaciones empresariales</description></item>
/// <item><description>Se recomienda cachear los Type objects para optimizar</description></item>
/// <item><description>Considerar Scoped lifetime para handlers con dependencias transitorias</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
/// <example>
/// Configuración en Program.cs:
/// <code>
/// builder.Services.AddScoped&lt;IDispatcher, InternalDispatcher&gt;();
/// builder.Services.AddScoped&lt;ICommandHandler&lt;CreateActivityCommand, Guid&gt;, CreateActivityHandler&gt;();
/// builder.Services.AddScoped&lt;IQueryHandler&lt;GetActivitiesQuery, PaginatedList&lt;DeviceActivityDto&gt;&gt;, GetActivitiesHandler&gt;();
/// </code>
/// </example>
public class InternalDispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    /// <inheritdoc/>
    public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(handlerType);
        return await (Task<TResponse>)handlerType.GetMethod("HandleAsync")!.Invoke(handler, [command, ct])!;
    }

    /// <inheritdoc/>
    public async Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(handlerType);
        return await (Task<TResponse>)handlerType.GetMethod("HandleAsync")!.Invoke(handler, [query, ct])!;
    }
}