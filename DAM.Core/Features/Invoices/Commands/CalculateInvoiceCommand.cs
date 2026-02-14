using DAM.Core.Abstractions;
using DAM.Core.DTOs.Invoices;

namespace DAM.Core.Features.Invoices.Commands;

/// <summary>
/// Comando para procesar y persistir el cálculo de una factura.
/// </summary>
public record CalculateInvoiceCommand(
    Guid ActivityId,
    bool UpdateActivityStatus
) : ICommand<InvoiceResponse>;