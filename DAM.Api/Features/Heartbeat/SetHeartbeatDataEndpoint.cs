using DAM.Core.Abstractions;
using DAM.Core.DTOs.Heartbeat;
using FastEndpoints;

namespace DAM.Api.Features.Heartbeat
{
    public class SetHeartbeatDataEndpoint(ICacheService cache, ILogger<SetHeartbeatDataEndpoint> logger)
    : Endpoint<HeartbeatDto>
    {
        public override void Configure()
        {
            Post("/service/heartbeat");
            AllowAnonymous();
        }

        public override async Task HandleAsync(HeartbeatDto req, CancellationToken ct)
        {
            // Guardamos en caché con una expiración ligeramente mayor al intervalo de envío
            // Si el servicio envía cada 30s, expiramos en 70s.
            var cacheKey = $"ServiceStatus_{req.ServiceName}";
            await cache.SetAsync(cacheKey, req, TimeSpan.FromSeconds(70));

            await Send.OkAsync(ct, ct);
        }
    }
}
