using DAM.Api.Base;
using DAM.Core.Constants;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Features.Activities.Queries;
using DAM.Core.Features.Invoices.Commands;
using DAM.Core.Interfaces;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.Invoices;

//public class CalculateInvoiceRequest
//{
//    public int ActivityId { get; set; }
//    public bool FinalizeActivity { get; set; } = true;
//}

/// <summary>
/// 💰 Calcula y persiste una factura para una actividad específica de dispositivo.
/// </summary>
/// <remarks>
/// <para>
/// <b>🔍 Detalles del endpoint:</b>
/// <list type="bullet">
/// <item><description><b>Método:</b> POST</description></item>
/// <item><description><b>Ruta:</b> /invoices/calculate</description></item>
/// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
/// <item><description><b>Roles permitidos:</b> Manager</description></item>
/// </list>
/// </para>
/// <para>
/// <b>🧮 Fórmula de cálculo:</b>
/// <code>
/// Total = TarifaBase + (GB_Procesados * TarifaPorGB)
/// </code>
/// </para>
/// <para>
/// <b>📊 Tarifas actuales:</b>
/// | Concepto | Valor |
/// |----------|-------|
/// | Tarifa Base | $5.00 USD |
/// | Tarifa por GB | $0.25 USD |
/// </para>
/// <para>
/// <b>⚠️ Operación atómica:</b>
/// Este endpoint es transaccional. Si la factura se genera exitosamente, la actividad
/// asociada se marca automáticamente como 'Completed' y no podrá ser facturada nuevamente.
/// </para>
/// </remarks>
public class CalculateInvoiceEndpoint(IDispatcher dispatcher, IDevicePersistenceService persistence)
    : BaseEndpoint<CalculateInvoiceCommand, InvoiceResponse>
{
    public override void Configure()
    {
        Post("/invoices/calculate");
        Roles("Manager");

        Description(x => x
            .Produces<InvoiceResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404)
            .WithTags("💰 Facturación"));

        Summary(s =>
        {
            s.Summary = "💰 [Facturación] Calcula y persiste factura de actividad";
            s.Description = """
                Procesa el cálculo de facturación para una actividad específica.
                **Este proceso es irreversible una vez finalizado.**
                """;
        });
    }

    public override async Task HandleAsync(CalculateInvoiceCommand req, CancellationToken ct)
    {
        //var command = new CalculateInvoiceCommand(req.ActivityId, req.FinalizeActivity);
        //var result = await dispatcher.SendAsync(command, ct);

        //await SendSuccessAsync(result, "✅ Factura procesada y guardada correctamente.", ct);

        // 1. Obtener actividad
        var activity = await dispatcher.QueryAsync(new GetActivityByIdQuery(req.DeviceActivityId), ct);

        if (activity == null)
        {
            AddError(Messages.Persistence.ActivityNotFound); // Usando tus constantes
            await SendErrorsAsync(404, ct);
            return;
        }

        // 2. Calcular usando la lógica que me suministraste (survivingFiles)
        var invoice = persistence.PersistInvoiceAsync(activity.ToEntity(), true);

        if (invoice == null)
        {
            // Escenario negativo: No hay archivos para cobrar
            AddError("No hay archivos netos para facturar en esta actividad.");
            await SendErrorsAsync(400, ct);
            return;
        }

        //try
        //{
        //    // 3. Intentar persistir (Comando que implementa la lógica de PersistInvoiceAsync)
        //    await dispatcher.SendAsync(new CreateInvoiceCommand(invoice), ct);

        //    // 4. Éxito: Mapeamos a DTO (Punto 6)
        //    var dto = new InvoiceDto(
        //        invoice.Id,
        //        invoice.SerialNumber,
        //        invoice.TotalAmount,
        //        invoice.Description,
        //        invoice.Timestamp
        //    );

        //    await SendSuccessAsync(dto, "Factura generada y persistida.", ct);
        //}
        //catch (Exception)
        //{
        //    AddError("Error crítico al persistir la factura calculada.");
        //    await SendErrorsAsync(500, ct);
        //}
    }
}