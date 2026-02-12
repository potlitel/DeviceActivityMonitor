using DAM.Api.Base;
using DAM.Core.Common;
using DAM.Core.DTOs.Common;
using DAM.Core.DTOs.DeviceActivity;
using DAM.Core.Features.Activities.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.DeviceActivity
{
    /// <summary>
    /// 📱 Obtiene el listado paginado de actividades de dispositivos de almacenamiento.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> GET</description></item>
    /// <item><description><b>Ruta:</b> /activities</description></item>
    /// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
    /// <item><description><b>Roles permitidos:</b> Manager, Worker</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>💾 ¿Qué es una actividad?</b>
    /// Una actividad representa el ciclo de vida completo de un dispositivo USB:
    /// <list type="number">
    /// <item><description><b>Inserción:</b> El dispositivo es conectado al puerto</description></item>
    /// <item><description><b>Uso:</b> El dispositivo permanece conectado y operativo</description></item>
    /// <item><description><b>Extracción:</b> El dispositivo es removido (opcional, null si aún está conectado)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>📊 Métricas incluidas:</b>
    /// - Capacidad total del dispositivo
    /// - Espacio disponible al inicio/fin
    /// - GB procesados (diferencia entre inicio y fin)
    /// - Duración de la actividad (si fue extraído)
    /// </para>
    /// </remarks>
    /// <response code="200">✅ Lista paginada de actividades</response>
    /// <response code="400">❌ Parámetros de paginación inválidos</response>
    /// <response code="401">❌ No autenticado o token inválido</response>
    /// <response code="403">❌ No autorizado - Se requiere rol Manager/Worker</response>
    public class GetActivitiesEndpoint(IDispatcher d) : BaseEndpoint<ActivityFilter, PaginatedList<DeviceActivityDto>>
    {
        public override void Configure()
        {
            Get("/activities");
            Roles("Manager", "Worker");

            Description(x => x
                .Produces<PaginatedList<DeviceActivityDto>>(200)
                .ProducesProblem(400)
                .ProducesProblem(401)
                .ProducesProblem(403)
                .WithTags("📱 Actividades")
                .WithDescription("""
                Recupera el historial de actividades de dispositivos USB.
                
                **📌 Filtros disponibles:**
                - `SerialNumber`: Número de serie del dispositivo
                - `Status`: Estado (Active/Completed)
                - `PageNumber`: Página actual
                - `PageSize`: Registros por página
                """));

            Summary(s =>
            {
                s.Summary = "📱 [Actividades] Obtiene listado paginado";
                s.Description = "Recupera todas las actividades de dispositivos con soporte para filtrado por número de serie y estado.";
                s.ExampleRequest = new ActivityFilter("SN-001", null, 1, 20);
            });
        }
        public override async Task HandleAsync(ActivityFilter r, CancellationToken ct)
        => await SendSuccessAsync(await d.QueryAsync(new GetActivitiesQuery(r), ct),
            "✅ Actividades recuperadas correctamente", ct);
    }
}
