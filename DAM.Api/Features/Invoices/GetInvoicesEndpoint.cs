using DAM.Api.Base;
using DAM.Core.Common;
using DAM.Core.DTOs.Common;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Features.Invoices.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.Invoices
{
    public class GetInvoicesEndpoint(IDispatcher d) : BaseEndpoint<InvoiceFilter, PaginatedList<InvoiceDto>>
    {
        public override void Configure() { Get("/invoices"); Roles("Manager"); }
        public override async Task HandleAsync(InvoiceFilter r, CancellationToken ct)
            => await SendSuccessAsync(await d.QueryAsync(new GetInvoicesQuery(r), ct), ct: ct);
    }
}
