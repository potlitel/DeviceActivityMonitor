using FastEndpoints;
using FluentValidation;

namespace DAM.Api.Features.Invoices;

//public class CalculateInvoiceValidator : Validator<CalculateInvoiceRequest>
//{
//    public CalculateInvoiceValidator()
//    {
//        RuleFor(x => x.ActivityId)
//            .NotEmpty().WithMessage("El ID de la actividad es obligatorio para el cálculo.");
//    }
//}