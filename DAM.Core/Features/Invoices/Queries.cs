using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.Invoices;

namespace DAM.Core.Features.Invoices.Queries;

/// <summary> Consulta el listado histórico de facturación. </summary>
public record GetInvoicesQuery(InvoiceFilter Filter) : IQuery<PaginatedList<InvoiceDto>>;

/// <summary> Detalle contable de una factura específica. </summary>
public record GetInvoiceByIdQuery(Guid Id) : IQuery<InvoiceDto?>;