using DAM.Api.Base;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Features.Invoices.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.Invoices
{
    /// <summary>
    /// 💰 Obtiene una factura específica por su identificador.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> GET</description></item>
    /// <item><description><b>Ruta:</b> /invoices/{id}</description></item>
    /// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
    /// <item><description><b>Roles permitidos:</b> Manager</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>🔐 Seguridad:</b>
    /// El acceso a facturación está restringido a perfiles administrativos debido a que contiene
    /// información fiscal, montos transaccionales y datos del cliente.
    /// </para>
    /// </remarks>
    /// <response code="200">✅ Factura encontrada y retornada</response>
    /// <response code="401">❌ No autenticado o token inválido</response>
    /// <response code="403">❌ No autorizado - Se requiere rol 'Manager'</response>
    /// <response code="404">❌ No se encontró factura con el ID especificado</response>
    public class GetInvoiceByIdEndpoint(IDispatcher d) : BaseEndpoint<GetByIdIntRequest, InvoiceDto>
    {
        public override void Configure() {

            Get("/invoices/{id}");
            Roles("Manager");

            Description(x => x
                .Produces<InvoiceDto>(200)
                .ProducesProblem(401)
                .ProducesProblem(403)
                .ProducesProblem(404)
                .WithTags("💰 Facturación"));

            Summary(s =>
            {
                s.Summary = "💰 [Facturación] Obtiene factura por ID";
                s.Description = "Recupera el desglose completo de una factura, incluyendo estados de pago y conceptos.";
                s.ExampleRequest = new GetByIdIntRequest(5050);
            });

        }
        public override async Task HandleAsync(GetByIdIntRequest r, CancellationToken ct)
        {
            var res = await d.QueryAsync(new GetInvoiceByIdQuery(r.Id), ct);

            if (res == null)
            {
                AddError($"❌ No se encontró factura con ID: {r.Id}");
                await SendErrorsAsync(404, ct);
                return;
            }

            await SendSuccessAsync(res, "✅ Factura recuperada correctamente", ct);
        }
    }
}
