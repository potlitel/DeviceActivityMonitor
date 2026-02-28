using DAM.Core.Abstractions;
using DAM.Core.DTOs.Heartbeat;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Core.Features.ServiceHeartBeats
{
    /// <summary>
    /// Comando para procesar y cachear el latido de un servicio.
    /// </summary>
    public record ServiceHeartbeatCmd(HeartbeatDto Data) : ICommand<bool>;

    /// <summary>
    /// Validador para el comando de Heartbeat de servicios worker.
    /// </summary>
    /// <remarks>
    /// Este validador asegura que todos los datos del latido sean consistentes y estén dentro de rangos esperados
    /// antes de ser procesados y cacheados por el sistema de monitoreo.
    /// </remarks>
    public class ServiceHeartbeatValidator : AbstractValidator<ServiceHeartbeatCmd>
    {
        public ServiceHeartbeatValidator()
        {
            // Validación del objeto contenedor
            RuleFor(x => x.Data)
                .NotNull()
                .WithMessage("Los datos del latido son obligatorios.");

            When(x => x.Data != null, () =>
            {
                // 📌 Identificación del servicio
                RuleFor(x => x.Data.ServiceName)
                    .NotEmpty()
                    .WithMessage("El nombre del servicio es obligatorio.")
                    .MaximumLength(100)
                    .WithMessage("El nombre del servicio no puede exceder los 100 caracteres.")
                    .Matches(@"^[a-zA-Z0-9\-_\.]+$")
                    .WithMessage("El nombre del servicio solo puede contener letras, números, guiones, underscores y puntos.");

                RuleFor(x => x.Data.MachineName)
                    .NotEmpty()
                    .WithMessage("El nombre de la máquina es obligatorio.")
                    .MaximumLength(50)
                    .WithMessage("El nombre de la máquina no puede exceder los 50 caracteres.")
                    .Matches(@"^[a-zA-Z0-9\-_]+$")
                    .WithMessage("El nombre de la máquina solo puede contener letras, números, guiones y underscores.");

                // 📊 Estado del servicio
                RuleFor(x => x.Data.Status)
                    .NotEmpty()
                    .WithMessage("El estado del servicio es obligatorio.")
                    .Must(status => new[] { "Running", "Stopping", "Starting", "Stopped", "Degraded" }.Contains(status))
                    .WithMessage("Estado inválido. Valores permitidos: Running, Stopping, Starting, Stopped, Degraded.");

                // 👁️ Monitoreo activo
                RuleFor(x => x.Data.ActiveWatchers)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("La cantidad de watchers activos debe ser mayor o igual a 0.")
                    .LessThanOrEqualTo(100)
                    .WithMessage("La cantidad de watchers activos no puede exceder 100 (posible fuga de recursos).");

                // 💻 Métricas de rendimiento - CPU
                RuleFor(x => x.Data.CpuUsagePercentage)
                    .InclusiveBetween(0, 100)
                    .WithMessage("El porcentaje de uso de CPU debe estar entre 0 y 100.");

                // 🧠 Métricas de rendimiento - Memoria
                RuleFor(x => x.Data.MemoryUsageMB)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("El uso de memoria debe ser mayor o igual a 0 MB.")
                    .LessThanOrEqualTo(16384) // 16GB máximo razonable para un worker
                    .WithMessage("El uso de memoria excede el límite esperado (16GB). Verificar posible fuga.");

                // ⏱️ Tiempo de actividad
                RuleFor(x => x.Data.UpTimeSeconds)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("El tiempo de actividad debe ser mayor o igual a 0 segundos.")
                    .LessThanOrEqualTo(31536000) // 1 año en segundos
                    .WithMessage("El tiempo de actividad excede 1 año. Verificar posible reinicio no reportado.");

                // 📦 Versión del ensamblado
                RuleFor(x => x.Data.Version)
                    .NotEmpty()
                    .WithMessage("La versión del ensamblado es obligatoria.")
                    .MaximumLength(50)
                    .WithMessage("La versión no puede exceder los 50 caracteres.")
                    .Matches(@"^\d+\.\d+\.\d+\.\d+$")
                    .WithMessage("La versión debe tener formato semver (ej. 1.2.3.4).");

                // 🕒 Timestamp
                RuleFor(x => x.Data.Timestamp)
                    .NotEmpty()
                    .WithMessage("El timestamp es obligatorio.")
                    .LessThanOrEqualTo(DateTime.UtcNow)
                    .WithMessage("El timestamp no puede ser futuro.")
                    .GreaterThan(DateTime.UtcNow.AddMinutes(-5))
                    .WithMessage("El timestamp es demasiado antiguo (mayor a 5 minutos). Posible reloj desincronizado.");
            });

            // Regla personalizada para validar consistencia de tiempos
            RuleFor(x => x.Data)
                .Must(HaveConsistentTiming)
                .WithMessage("El timestamp del latido no es consistente con el uptime reportado.");
        }

        /// <summary>
        /// Validación personalizada para asegurar que el timestamp y el uptime sean consistentes.
        /// </summary>
        private bool HaveConsistentTiming(HeartbeatDto data)
        {
            if (data == null) return false;

            // El timestamp no debería ser anterior al inicio del servicio
            var estimatedStartTime = data.Timestamp.AddSeconds(-data.UpTimeSeconds);
            var timeDifference = Math.Abs((DateTime.UtcNow - estimatedStartTime).TotalSeconds);

            // Permitimos una diferencia de hasta 5 minutos (por posibles reinicios)
            return timeDifference <= 300;
        }
    }

    /// <summary>
    /// Validador extendido con reglas de negocio adicionales.
    /// https://chat.deepseek.com/a/chat/s/147973cc-23a7-40c3-bd4a-8daeaf443dd4
    /// </summary>
    //public class ServiceHeartbeatBusinessValidator : AbstractValidator<ServiceHeartbeatCmd>
    //{
    //    private readonly IServiceRegistry _serviceRegistry;

    //    public ServiceHeartbeatBusinessValidator(IServiceRegistry serviceRegistry)
    //    {
    //        _serviceRegistry = serviceRegistry;

    //        // Validación de negocio: El servicio debe estar registrado
    //        RuleFor(x => x.Data.MachineName)
    //            .MustAsync(async (machineName, ct) =>
    //                await _serviceRegistry.IsRegisteredAsync(machineName, ct))
    //            .WithMessage("El servicio no está registrado en el sistema.");

    //        // Validación de umbrales críticos
    //        When(x => x.Data?.Status == "Running", () =>
    //        {
    //            RuleFor(x => x.Data.CpuUsagePercentage)
    //                .LessThanOrEqualTo(90)
    //                .WithMessage("Uso de CPU crítico (>90%) en servicio activo.");

    //            RuleFor(x => x.Data.MemoryUsageMB)
    //                .LessThanOrEqualTo(8192) // 8GB
    //                .WithMessage("Uso de memoria crítico (>8GB) en servicio activo.");
    //        });
    //    }
    //}
}
