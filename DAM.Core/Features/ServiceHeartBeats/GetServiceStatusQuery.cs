using DAM.Core.Abstractions;
using DAM.Core.DTOs.Heartbeat;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Core.Features.ServiceHeartBeats
{
    /// <summary>
    /// Consulta para obtener el estado actual de un servicio específico desde la caché.
    /// </summary>
    /// <param name="MachineName">Nombre de la máquina a consultar.</param>
    public record GetServiceStatusQuery(string MachineName) : IQuery<ServiceStatusResponse?>;

    /// <summary>
    /// DTO de respuesta para el estado del servicio, incluyendo lógica de frescura de datos.
    /// </summary>
    public record ServiceStatusResponse(
        HeartbeatDto Data,
        string SummaryStatus, // ONLINE, STALE, OFFLINE
        double LastSeenSecondsAgo,
        bool IsStale
    );

    public class GetServiceStatusValidator : AbstractValidator<GetServiceStatusQuery>
    {
        public GetServiceStatusValidator()
        {
            RuleFor(x => x.MachineName).NotEmpty().WithMessage("Se debe especificar el nombre de la máquina.");
        }
    }
}
