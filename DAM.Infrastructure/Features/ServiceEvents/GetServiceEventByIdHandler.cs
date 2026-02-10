using DAM.Core.Abstractions;
using DAM.Core.DTOs.Events;
using DAM.Core.Features.Events.Queries;
using DAM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Infrastructure.Features.ServiceEvents
{
    public class GetServiceEventByIdHandler(IServiceEventRepository repository) : IQueryHandler<GetServiceEventByIdQuery, ServiceEventDto?>
    {
        public async Task<ServiceEventDto?> HandleAsync(GetServiceEventByIdQuery q, CancellationToken ct)
        {
            var x = await repository.GetByIdAsync(q.Id, ct); // Aquí el ID es int, ajustar interfaz si es necesario
            return x == null ? null : new ServiceEventDto(x.Id, x.Message);
        }
    }
}
