using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Features.Invoices.Queries;
using DAM.Core.Interfaces;
using DAM.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Infrastructure.Features.Invoices.Handlers
{
    public class GetInvoicesHandler(IInvoiceRepository repository) : IQueryHandler<GetInvoicesQuery, PaginatedList<InvoiceDto>>
    {
        public async Task<PaginatedList<InvoiceDto>> HandleAsync(GetInvoicesQuery q, CancellationToken ct)
            => await repository.GetAllQueryable()
                .OrderByDescending(x => x.Timestamp)
                .ToPaginatedListAsync(q.Filter.PageNumber, q.Filter.PageSize, x => new InvoiceDto(x.Id, x.SerialNumber, x.Timestamp, x.TotalAmount, x.Description), ct);
    }
}
