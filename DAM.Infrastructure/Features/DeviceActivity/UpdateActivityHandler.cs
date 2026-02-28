using DAM.Core.Abstractions;
using DAM.Core.Interfaces;
using DAM.Core.DTOs.DeviceActivity;
using Microsoft.Extensions.Logging;
using DAM.Core.Features.DeviceActivity.Commands;

namespace DAM.Infrastructure.Features.DeviceActivity
{
    /// <summary>
    /// Manejador para actualizar estados de actividad (ej. al extraer el USB).
    /// </summary>
    public class UpdateActivityHandler(
        IUnitOfWork uow,
        ILogger<UpdateActivityHandler> logger)
        : ICommandHandler<UpdateActivityCmd, bool>
    {
        public async Task<bool> HandleAsync(UpdateActivityCmd cmd, CancellationToken ct)
        {
            try
            {
                var entity = await uow.Activities.GetByIdAsync(cmd.Id, ct);
                if (entity == null)
                {
                    logger.LogWarning("No se encontró actividad con ID {Id} para actualizar", cmd.Id);
                    return false;
                }

                cmd.UpdateEntity(entity);
                await uow.Activities.UpdateActivityAsync(entity);
                await uow.SaveChangesAsync();

                logger.LogInformation("Actividad {Id} actualizada exitosamente. Estado: {Status}",
                    cmd.Id, cmd.Status);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar actividad {Id}", cmd.Id);
                throw;
            }
        }
    }
}
