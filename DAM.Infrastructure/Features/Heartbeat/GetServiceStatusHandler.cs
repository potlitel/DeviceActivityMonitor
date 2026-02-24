using DAM.Core.Abstractions;
using DAM.Core.DTOs.Heartbeat;
using DAM.Core.Features.ServiceHeartBeats;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Infrastructure.Features.Heartbeat
{
    /// <summary>
    /// Manejador de consultas que recupera el estado y calcula la "frescura" del servicio.
    /// </summary>
    public class GetServiceStatusHandler(ICacheService cache)
        : IQueryHandler<GetServiceStatusQuery, ServiceStatusResponse?>
    {
        public async Task<ServiceStatusResponse?> HandleAsync(GetServiceStatusQuery query, CancellationToken ct)
        {
            var data = await cache.GetAsync<HeartbeatDto>($"ServiceStatus_{query.MachineName}");

            if (data == null) return null;

            var secondsAgo = (DateTime.Now - data.Timestamp).TotalSeconds;

            // Si el dato tiene más de 35 segundos, lo consideramos "Stale" (viejo/retrasado)
            bool isStale = secondsAgo > 35;
            string summary = isStale ? "STALE" : "ONLINE";

            return new ServiceStatusResponse(
                Data: data,
                SummaryStatus: summary,
                LastSeenSecondsAgo: Math.Round(secondsAgo, 1),
                IsStale: isStale
            );
        }
    }
}
