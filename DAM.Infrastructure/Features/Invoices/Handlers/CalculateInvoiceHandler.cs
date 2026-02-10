//using DAM.Core.Abstractions;
//using DAM.Core.DTOs.Invoices;
//using DAM.Core.Enums;
//using DAM.Core.Features.Invoices.Commands;
//using DAM.Core.Interfaces;
//using DAM.Infrastructure.Persistence;
//using Microsoft.Extensions.Logging;

//namespace DAM.Infrastructure.Features.Invoices.Handlers;

//public class CalculateInvoiceHandler(
//    IUnitOfWork uow,
//    IInvoiceCalculator calculator,
//    ILogger<CalculateInvoiceHandler> logger)
//    : ICommandHandler<CalculateInvoiceCommand, InvoiceResponse>
//{
//    public async Task<InvoiceResponse> HandleAsync(CalculateInvoiceCommand command, CancellationToken ct)
//    {
//        //TODO: Usar el servicio ya implementado en el worker.

//        #region ToDelete
//        // 1. Obtener la actividad con sus archivos (Unit of Work)
//        //var activity = await uow.Activities.GetByIdAsync(command.ActivityId, ct);

//        //if (activity == null)
//        //    throw new InvalidOperationException("La actividad especificada no existe.");

//        //// 2. Ejecutar lógica de cálculo (Domain Service)
//        //var invoice = calculator.CalculateInvoice(activity);

//        //try
//        //{
//        //    await uow.BeginTransactionAsync();

//        //    // 3. Persistir factura
//        //    await uow.Invoices.AddAsync(invoice, ct);

//        //    // 4. Actualizar estado si se solicita (Máquina de Estados)
//        //    if (command.UpdateActivityStatus)
//        //    {
//        //        activity.Status = ActivityStatus.Completed;
//        //        await uow.Activities.UpdateAsync(activity, ct);
//        //    }

//        //    await uow.SaveChangesAsync(ct);
//        //    await uow.CommitTransactionAsync();

//        //    logger.LogInformation("Factura {Num} generada para SN: {SN}",
//        //        invoice.InvoiceNumber, activity.SerialNumber);

//        //    return new InvoiceResponse(
//        //        invoice.InvoiceNumber,
//        //        activity.SerialNumber,
//        //        invoice.TotalAmount,
//        //        activity.MegabytesCopied + activity.MegabytesDeleted,
//        //        DateTime.UtcNow
//        //    );
//        //}
//        //catch (Exception ex)
//        //{
//        //    await uow.RollbackTransactionAsync();
//        //    logger.LogError(ex, "Fallo crítico al persistir factura para actividad {Id}", command.ActivityId);
//        //    throw;
//        //}
//        #endregion

//        return await Task.FromResult(new InvoiceResponse { });
//    }
//}