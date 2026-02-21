using DAM.Core.Abstractions;
using DAM.Core.DTOs.DevicePresence;
using DAM.Core.Features.Presence.Queries;
using DAM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAM.Infrastructure.Features.DevicePresence
{
    /// <summary>
    /// Handler para obtener un evento de presencia por su identificador.
    /// </summary>
    /// <remarks>
    /// <b>TODO:</b> Incluir la relación con DeviceActivity cuando esté disponible
    /// para proporcionar contexto completo del evento.
    /// </remarks>
    public class GetPresenceByIdHandler(IPresenceRepository repository)
        : IQueryHandler<GetPresenceByIdQuery, DevicePresenceDto?>
    {
        public async Task<DevicePresenceDto?> HandleAsync(
            GetPresenceByIdQuery query,
            CancellationToken cancellationToken)
        {
            var entity = await repository.GetByIdAsync(query.Id, cancellationToken, 
                                                       query => query.Include(x => x.DeviceActivity));

            return entity == null ? null : DevicePresenceDto.FromEntity(entity);
        }
    }
}
