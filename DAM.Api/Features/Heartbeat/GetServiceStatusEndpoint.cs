using DAM.Api.Base;
using DAM.Core.Abstractions;
using DAM.Core.DTOs.Heartbeat;

namespace DAM.Api.Features.Heartbeat
{
    public class GetServiceStatusEndpoint(ICacheService cache) : BaseEndpoint<string, HeartbeatDto?>
    {
        public override void Configure()
        {
            Get("/service/status/{serviceName}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(string req, CancellationToken ct)
        {
            //var serviceName = Route<string>("serviceName") ?? "DAM";
            var serviceName = req;
            var status = await cache.GetAsync<HeartbeatDto>($"ServiceStatus_{serviceName}");

            if (status == null)
            {
                await SendSuccessAsync(null, "El servicio parece estar OFFLINE o no ha reportado actividad.", ct);
                return;
            }

            await SendSuccessAsync(status, "Servicio operando correctamente.", ct);
        }
    }
}
