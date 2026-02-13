using DAM.Api.Base;
using DAM.Core.DTOs.DeviceActivity;
using DAM.Core.Features.Activities.Queries;
using DAM.Infrastructure.CQRS;
using FastEndpoints;

namespace DAM.Api.Features.DeviceActivity
{
    /// <summary>
    /// 📱 Obtiene una actividad específica de dispositivo por su identificador.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> GET</description></item>
    /// <item><description><b>Ruta:</b> /activities/{id}</description></item>
    /// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
    /// <item><description><b>Roles permitidos:</b> Manager</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>🔐 Seguridad:</b>
    /// Este endpoint está restringido al rol Manager ya que expone métricas
    /// detalladas que podrían ser consideradas sensibles.
    /// </para>
    /// </remarks>
    /// <response code="200">✅ Actividad encontrada y retornada</response>
    /// <response code="401">❌ No autenticado o token inválido</response>
    /// <response code="403">❌ No autorizado - Se requiere rol 'Manager'</response>
    /// <response code="404">❌ No se encontró actividad con el ID especificado</response>
    public class GetActivityByIdEndpoint(IDispatcher d) : BaseEndpoint<GetByIdIntRequest, DeviceActivityDto>
    {
        public override void Configure() {

            Get("/activities/{id}");
            Roles("Manager");

            Description(x => x
                .Produces<DeviceActivityDto>(200)
                .ProducesProblem(401)
                .ProducesProblem(403)
                .ProducesProblem(404)
                .WithTags("📱 Actividades"));

            Summary(s =>
            {
                s.Summary = "📱 [Actividades] Obtiene una actividad por ID";
                s.Description = "Recupera los detalles completos de una actividad específica utilizando su identificador.";
                s.ExampleRequest = new GetByIdIntRequest(15);
            });

        }
        public override async Task HandleAsync(GetByIdIntRequest r, CancellationToken ct)
        {
            var res = await d.QueryAsync(new GetActivityByIdQuery(r.Id), ct);

            if (res == null)
            {
                AddError($"❌ No se encontró actividad con ID: {r.Id}");
                await SendErrorsAsync(404, ct);
                return;
            }

            await SendSuccessAsync(res, "✅ Actividad recuperada correctamente", ct);
        }
    }
}
