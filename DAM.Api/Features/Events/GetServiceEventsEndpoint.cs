using DAM.Api.Base;
using DAM.Core.Common;
using DAM.Core.DTOs.Events;
using DAM.Core.Features.Events.Queries;
using DAM.Infrastructure.CQRS;

/// <summary>
/// 📊 Obtiene los eventos de la "caja negra" del sistema (logs del servicio Windows).
/// </summary>
/// <remarks>
/// <para>
/// <b>🔍 Detalles del endpoint:</b>
/// <list type="bullet">
/// <item><description><b>Método:</b> GET</description></item>
/// <item><description><b>Ruta:</b> /system/events</description></item>
/// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
/// <item><description><b>Roles permitidos:</b> Manager</description></item>
/// </list>
/// </para>
/// <para>
/// <b>🖥️ ¿Qué es el "Servicio de Monitoreo en Segundo Plano"?</b>
/// Es un Windows Service que se ejecuta en segundo plano para la detección de dispositivos externos
/// y registro de actividad de E/S de almacenamiento USB. Garantiza la persistencia resiliente de datos
/// incluso en condiciones de alta concurrencia o fallos temporales.
/// </para>
/// <para>
/// <b>📋 Este endpoint expone los logs internos de dicho servicio:</b>
/// <list type="bullet">
/// <item><description>✅ Inicio/parada del servicio</description></item>
/// <item><description>🔌 Detección de dispositivos conectados/removidos</description></item>
/// <item><description>⚠️ Advertencias de espacio en disco bajo</description></item>
/// <item><description>❌ Errores de lectura/escritura</description></item>
/// <item><description>🔄 Intentos de reintento por fallos transitorios</description></item>
/// </list>
/// </para>
/// </remarks>
public class GetServiceEventsEndpoint(IDispatcher dispatcher)
    : BaseEndpoint<ServiceEventFilter, PaginatedList<ServiceEventDto>>
{
    public override void Configure()
    {
        Get("/system/events");
        Roles("Manager");

        Description(x => x
            .Produces<PaginatedList<ServiceEventDto>>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .WithTags("📊 Sistema/Eventos"));

        Summary(s =>
        {
            s.Summary = "📊 [Sistema] Consulta la 'Caja Negra' del servicio Windows";
            s.Description = """
                Muestra logs detallados del funcionamiento interno del Worker Service y el Watcher.
                
                **🔍 Filtros disponibles:**
                - `Level`: Nivel de severidad (Information, Warning, Error)
                - `Source`: Componente origen (DeviceWatcher, WorkerService, etc.)
                - `PageNumber/PageSize`: Paginación estándar
                """;
        });
    }

    public override async Task HandleAsync(ServiceEventFilter req, CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetServiceEventsQuery(req), ct);
        await SendSuccessAsync(result, "✅ Eventos de sistema recuperados correctamente", ct);
    }
}