using DAM.Api.Base;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Features.Invoices.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.Invoices
{
    public class GetInvoiceByIdEndpoint(IDispatcher d) : BaseEndpoint<GetByIdRequest, InvoiceDto>
    {
        public override void Configure() { Get("/invoices/{id}"); Roles("Manager"); }
        public override async Task HandleAsync(GetByIdRequest r, CancellationToken ct)
        {
            var res = await d.QueryAsync(new GetInvoiceByIdQuery(r.Id), ct);
            if (res == null) await SendErrorsAsync(404, ct); else await SendSuccessAsync(res, ct: ct);
        }
    }
}
