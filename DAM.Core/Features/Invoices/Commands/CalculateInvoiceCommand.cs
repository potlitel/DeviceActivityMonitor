using DAM.Core.Abstractions;
using DAM.Core.DTOs.Invoices;
using FluentValidation;

namespace DAM.Core.Features.Invoices.Commands;

/// <summary>
/// Comando para procesar y persistir el cálculo de una factura.
/// </summary>
public record CalculateInvoiceCommand(
    string SerialNumber,
    DateTime Timestamp,
    decimal TotalAmount,
    string Description,
    int DeviceActivityId
) : ICommand<InvoiceResponse>;

public class CreateInvoiceValidator : AbstractValidator<CalculateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("El Serial Number es obligatorio.");

        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("La fecha y hora de la factura es obligatoria.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("El monto total debe ser mayor a 0.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción de la factura es obligatoria.")
            .MaximumLength(500).WithMessage("La descripción no puede exceder los 500 caracteres.");

        RuleFor(x => x.DeviceActivityId)
            .GreaterThan(0).WithMessage("Se requiere un ID de actividad de dispositivo válido.");
    }
}