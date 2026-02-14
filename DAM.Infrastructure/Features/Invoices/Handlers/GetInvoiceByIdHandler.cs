using DAM.Core.Abstractions;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Features.Invoices.Queries;
using DAM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Infrastructure.Features.Invoices.Handlers
{
    public class GetInvoiceByIdHandler(IInvoiceRepository repository) : IQueryHandler<GetInvoiceByIdQuery, InvoiceDto?>
    {
        public async Task<InvoiceDto?> HandleAsync(GetInvoiceByIdQuery q, CancellationToken ct)
        {
            var x = await repository.GetByIdAsync(q.Id, ct);
            return x == null ? null : new InvoiceDto(x.Id, x.SerialNumber, x.Timestamp, x.TotalAmount, x.Description);
        }
    }
}
