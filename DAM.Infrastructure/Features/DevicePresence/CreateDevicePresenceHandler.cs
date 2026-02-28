using DAM.Core.Abstractions;
using DAM.Core.Interfaces;
using DAM.Core.DTOs.DevicePresence;
using Microsoft.Extensions.Logging;
using DAM.Core.Features.DevicePresence.Commands;

namespace DAM.Infrastructure.Features.DevicePresence
{
    /// <summary>
    /// Manejador para registros de presencia.
    /// </summary>
    public class CreateDevicePresenceHandler(
        IUnitOfWork uow,
        ILogger<CreateDevicePresenceHandler> logger)
        : ICommandHandler<CreateDevicePresenceCmd, int>
    {
        public async Task<int> HandleAsync(CreateDevicePresenceCmd cmd, CancellationToken ct)
        {
            try
            {
                var entity = cmd.ToEntity();
                await uow.Activities.AddDevicePresenceAsync(entity);
                await uow.SaveChangesAsync();

                logger.LogDebug("Presencia registrada para dispositivo {Serial} en actividad {ActivityId}",
                    cmd.SerialNumber, cmd.DeviceActivityId);

                return entity.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al registrar presencia para dispositivo {Serial}", cmd.SerialNumber);
                throw;
            }
        }
    }
}
