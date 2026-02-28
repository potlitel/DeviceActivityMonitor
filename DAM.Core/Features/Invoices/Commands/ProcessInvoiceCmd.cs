using DAM.Core.Abstractions;
using DAM.Core.DTOs.Invoices;
using FluentValidation;

namespace DAM.Core.Features.Invoices.Commands
{
    /// <summary>
    /// Comando para procesar la facturación de una actividad.
    /// </summary>
    /// <returns>Detalles de la factura procesada</returns>
    public record ProcessInvoiceCmd(
        int ActivityId,
        bool FinalizeActivity
    ) : ICommand<InvoiceResponse>;

    public class ProcessInvoiceValidator : AbstractValidator<ProcessInvoiceCmd>
    {
        public ProcessInvoiceValidator()
        {
            RuleFor(x => x.ActivityId)
                .GreaterThan(0).WithMessage("ID de actividad inválido.");
        }
    }
}
