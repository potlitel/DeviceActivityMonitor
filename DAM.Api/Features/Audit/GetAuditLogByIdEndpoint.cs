using DAM.Api.Base;
using DAM.Core.DTOs.AuditLogs;
using DAM.Core.Features.Audit.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.Audit
{
    /// <summary>
    /// 📋 Obtiene una entrada específica del historial de auditoría por su identificador único.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> GET</description></item>
    /// <item><description><b>Ruta:</b> /audit/{id}</description></item>
    /// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
    /// <item><description><b>Roles permitidos:</b> Manager</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>📌 Casos de uso:</b>
    /// Este endpoint es utilizado por el módulo de cumplimiento normativo y auditoría interna
    /// para revisar transacciones específicas que requieren trazabilidad forense.
    /// </para>
    /// <para>
    /// <b>🔐 Seguridad:</b>
    /// Solo los usuarios con rol 'Manager' tienen acceso a este endpoint. Cada consulta
    /// genera automáticamente un registro en el log de auditoría.
    /// </para>
    /// </remarks>
    /// <response code="200">✅ Auditoría encontrada y retornada correctamente</response>
    /// <response code="401">❌ No autenticado o token inválido/vencido</response>
    /// <response code="403">❌ No autorizado - Se requiere rol 'Manager'</response>
    /// <response code="404">❌ No se encontró ningún registro con el ID especificado</response>
    public class GetAuditLogByIdEndpoint(IDispatcher d) : BaseEndpoint<GetByIdRequest, AuditLogDto>
    {
        public override void Configure()
        {
            Get("/audit/{id}");
            Roles("Manager");

            Description(x => x
                .Produces<AuditLogDto>(200)
                .ProducesProblem(401)
                .ProducesProblem(403)
                .ProducesProblem(404)
                .WithTags("📋 Auditoría"));

            Summary(s =>
            {
                s.Summary = "📋 [Auditoría] Obtiene un registro específico por ID";
                s.Description = "Recupera los detalles completos de una entrada de auditoría utilizando su identificador único (GUID).";
                s.ExampleRequest = new GetByIdRequest(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
                //s.ResponseExamples[200] = new AuditLogDto
                //{
                //    Id = Guid.NewGuid(),
                //    UserId = Guid.NewGuid().ToString(),
                //    //Username = "jperez",
                //    Action = "UserLogin",
                //    Resource = "/auth/login",
                //    HttpMethod = "POST",
                //    TimestampUtc = DateTime.UtcNow,
                //    //IpAddress = "192.168.1.100",
                //    //UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
                //};
            });
        }

        public override async Task HandleAsync(GetByIdRequest r, CancellationToken ct)
        {
            var res = await d.QueryAsync(new GetAuditLogByIdQuery(r.Id), ct);

            if (res == null)
            {
                AddError($"❌ No se encontró ningún registro de auditoría con ID: {r.Id}");
                await SendErrorsAsync(404, ct);
                return;
            }

            await SendSuccessAsync(res, ct: ct);
        }
    }
}
