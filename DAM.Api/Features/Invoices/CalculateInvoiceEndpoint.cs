using DAM.Api.Base;
using DAM.Core.Abstractions;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Features.Invoices.Commands;
using DAM.Infrastructure.CQRS;
using FastEndpoints;

namespace DAM.Api.Features.Invoices;

public class CalculateInvoiceRequest
{
    public Guid ActivityId { get; set; }
    public bool FinalizeActivity { get; set; } = true;
}

public class CalculateInvoiceEndpoint(IDispatcher dispatcher)
    : BaseEndpoint<CalculateInvoiceRequest, InvoiceResponse>
{
    public override void Configure()
    {
        Post("/invoices/calculate");
        Roles("Manager"); // Solo los Managers pueden forzar el cálculo manual
        Summary(s => {
            s.Summary = "Calcula y persiste la factura de una actividad específica.";
            s.Description = "Este proceso es atómico y actualiza el estado de la actividad a 'Completed'.";
        });
    }

    public override async Task HandleAsync(CalculateInvoiceRequest req, CancellationToken ct)
    {
        //Inyectar el servicio ya implementado que usa el contrato IInvoiceCalculator!!!
        var command = new CalculateInvoiceCommand(req.ActivityId, req.FinalizeActivity);

        // El dispatcher localiza el Handler y ejecuta la lógica
        var result = await dispatcher.SendAsync(command, ct);

        await SendSuccessAsync(result, "Factura procesada y guardada correctamente.", ct);
    }
}