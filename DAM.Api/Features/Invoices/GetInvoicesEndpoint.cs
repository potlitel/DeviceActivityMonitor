using DAM.Api.Base;
using DAM.Core.Common;
using DAM.Core.DTOs.Common;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Features.Invoices.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.Invoices
{
    /// <summary>
    /// 💰 Obtiene el listado paginado de facturas generadas.
    /// </summary>
    public class GetInvoicesEndpoint(IDispatcher d) : BaseEndpoint<InvoiceFilter, PaginatedList<InvoiceDto>>
    {
        public override void Configure() {

            Get("/invoices");
            Roles("Manager");

            Description(x => x
                .Produces<PaginatedList<InvoiceDto>>(200)
                .ProducesProblem(400)
                .ProducesProblem(401)
                .ProducesProblem(403)
                .WithTags("💰 Facturación"));

        }
        public override async Task HandleAsync(InvoiceFilter r, CancellationToken ct)
        => await SendSuccessAsync(await d.QueryAsync(new GetInvoicesQuery(r), ct),
            "✅ Facturas recuperadas correctamente", ct);
    }
}
