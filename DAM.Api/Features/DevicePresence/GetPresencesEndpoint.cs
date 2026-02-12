using DAM.Api.Base;
using DAM.Core.Common;
using DAM.Core.DTOs.Common;
using DAM.Core.DTOs.DevicePresence;
using DAM.Core.Features.Presence.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.DevicePresence
{
    /// <summary>
    /// 👤 Obtiene el listado paginado de eventos de presencia de dispositivos.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> GET</description></item>
    /// <item><description><b>Ruta:</b> /presence</description></item>
    /// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
    /// <item><description><b>Roles permitidos:</b> Manager</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>📊 Casos de uso:</b>
    /// - Monitorización en tiempo real de dispositivos conectados
    /// - Análisis de patrones de uso de dispositivos
    /// - Detección de dispositivos no autorizados
    /// - Auditoría de accesos a puertos USB
    /// </para>
    /// </remarks>
    public class GetPresencesEndpoint(IDispatcher d) : BaseEndpoint<PresenceFilter, PaginatedList<DevicePresenceDto>>
    {
        public override void Configure() {

            Get("/presence");
            Roles("Manager");

            Description(x => x
                .Produces<PaginatedList<DevicePresenceDto>>(200)
                .ProducesProblem(400)
                .ProducesProblem(401)
                .ProducesProblem(403)
                .WithTags("👤 Presencia")
                .WithDescription("""
                # 👤 Endpoint de Presencia
                
                ## ✅ **Ejemplo CORRECTO (INT):**
                ```
                GET /api/presence?ActivityId=5&PageNumber=1&PageSize=10
                ```
                
                ## ❌ **Ejemplo INCORRECTO (GUID):**
                ```
                GET /api/presence?ActivityId=550e8400-e29b-41d4-a716-446655440000
                ```
                
                > ⚠️ **IMPORTANTE**: ActivityId es un número entero (INT), NO un GUID.
                > Si usas GUID obtendrás error 400.
                """));

            Summary(s =>
            {
                s.Summary = "👤 [Presencia] Obtiene eventos de presencia (INT, no GUID)";
                s.Description = """
                Retorna eventos de presencia de dispositivos.
                
                **🔍 Filtros:**
                - `ActivityId` **(INT, opcional)**: ID de la actividad (ej: 1, 5, 23)
                - `PageNumber` (int): Página actual
                - `PageSize` (int): Registros por página
                
                **⚠️ ERROR COMÚN:**
                NO usar GUID para ActivityId. Siempre usar números enteros.
                """;
                s.ExampleRequest = "new PresenceFilter(ActivityId: 5, PageNumber: 1, PageSize: 10);";
                //s.ResponseExamples[200] = "new PaginatedList<DevicePresenceDto>
                //{
                //    Items = new List<DevicePresenceDto>
                //{
                //    new(1, "SN-001", DateTime.UtcNow.AddHours(-2), 5),
                //    new(2, "SN-001", DateTime.UtcNow.AddHours(-1), 5)
                //},
                //    PageNumber = 1,
                //    PageSize = 10,
                //    TotalCount = 2
                //};"
            });

        }
        public override async Task HandleAsync(PresenceFilter r, CancellationToken ct)
            => await SendSuccessAsync(await d.QueryAsync(new GetPresencesQuery(r), ct), ct: ct);
    }
}
