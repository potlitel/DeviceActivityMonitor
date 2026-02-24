using DAM.Core.Abstractions;
using DAM.Core.Features.DeviceActivity.Commands;
using DAM.Core.Interfaces;
using DAM.Core.DTOs.DeviceActivity;
using Microsoft.Extensions.Logging;

namespace DAM.Infrastructure.Features.DeviceActivity
{
    /// <summary>
    /// Manejador para persistir una nueva actividad.
    /// </summary>
    /// <summary>
    /// Manejador para persistir una nueva actividad.
    /// </summary>
    public class CreateActivityHandler(
        IUnitOfWork uow,
        ILogger<CreateActivityHandler> logger)
        : ICommandHandler<CreateActivityCmd, int>
    {
        public async Task<int> HandleAsync(CreateActivityCmd cmd, CancellationToken ct)
        {
            try
            {
                var entity = cmd.ToEntity();
                await uow.Activities.AddActivityAsync(entity);
                await uow.SaveChangesAsync();

                logger.LogInformation("Actividad creada exitosamente para dispositivo {Serial} con ID {Id}",
                    cmd.SerialNumber, entity.Id);

                return entity.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear actividad para {Serial}", cmd.SerialNumber);
                throw; // Relanzamos para que el manejador de excepciones global lo procese
            }
        }
    }

}
