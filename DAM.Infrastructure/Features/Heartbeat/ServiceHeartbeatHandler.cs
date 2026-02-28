using DAM.Core.Abstractions;
using DAM.Core.DTOs.Heartbeat;
using DAM.Core.Features.ServiceHeartBeats;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Infrastructure.Features.Heartbeat
{
    /// <summary>
    /// Manejador de comandos para registrar el latido en la caché distribuida o local.
    /// </summary>
    /// <remarks>
    /// Implementa una política de expiración de 45 segundos para cubrir el gap de envío de 30 segundos del Worker.
    /// </remarks>
    public class ServiceHeartbeatHandler(ICacheService cache, ILogger<ServiceHeartbeatHandler> logger)
        : ICommandHandler<ServiceHeartbeatCmd, bool>
    {
        private const string CacheKeyPrefix = "ServiceStatus_";

        public async Task<bool> HandleAsync(ServiceHeartbeatCmd cmd, CancellationToken ct)
        {
            try
            {
                string key = $"{CacheKeyPrefix}{cmd.Data.MachineName}";

                // Almacenamos con un margen de 15 segundos sobre el intervalo del worker (30s)
                await cache.SetAsync(key, cmd.Data, TimeSpan.FromSeconds(45));

                logger.LogTrace("Heartbeat actualizado para la máquina: {Machine}", cmd.Data.MachineName);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al escribir latido en caché para {Machine}", cmd.Data.MachineName);
                return false;
            }
        }
    }
}
