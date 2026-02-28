using DAM.Core.Abstractions;
using DAM.Core.Interfaces;
using DAM.Core.DTOs.ServiceEvents;
using Microsoft.Extensions.Logging;
using DAM.Core.Features.ServiceEvents.Commands;

namespace DAM.Infrastructure.Features.ServiceEvents
{
    /// <summary>
    /// Manejador para eventos de auditoría del servicio.
    /// </summary>
    public class CreateServiceEventHandler(
        IUnitOfWork uow,
        ILogger<CreateServiceEventHandler> logger)
        : ICommandHandler<CreateServiceEventCmd, int>
    {
        public async Task<int> HandleAsync(CreateServiceEventCmd cmd, CancellationToken ct)
        {
            try
            {
                var entity = cmd.ToEntity();
                await uow.Activities.AddServiceEventAsync(entity);
                await uow.SaveChangesAsync();

                logger.LogInformation("Evento de servicio registrado: {EventType} - {Message}",
                    cmd.EventType, cmd.Message);

                return entity.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al registrar evento de servicio: {EventType}", cmd.EventType);
                throw;
            }
        }
    }
}
