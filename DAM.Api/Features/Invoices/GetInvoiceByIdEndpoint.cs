using DAM.Api.Base;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Features.Invoices.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.Invoices
{
    /// <summary>
    /// 💰 Obtiene una factura específica por su identificador.
    /// </summary>

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
