using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// Monitor de carga computacional del proceso.
/// A diferencia de la memoria, el uso de CPU requiere dos muestras en el tiempo para calcular un diferencial.
/// Implementa lógica multi-core para normalizar el porcentaje de uso sobre la capacidad total del sistema.
/// </summary>
public class ProcessCpuHealthCheck : IHealthCheck
{
    private readonly double _maximumCpuPercentage;
    private readonly Process _process;
    private readonly TimeSpan _sampleWindow;
    private readonly ILogger<ProcessCpuHealthCheck>? _logger;

    // Estado interno para el cálculo diferencial del tiempo de procesador.
    private TimeSpan _lastCpuTime;
    private DateTime _lastSampleTime;

    /// <param name="maximumCpuPercentage">Límite de carga (0-100). Valores superiores indican saturación de hilos o procesos costosos.</param>
    /// <param name="sampleWindow">Ventana de tiempo mínima para considerar válida una muestra de CPU.</param>
    public ProcessCpuHealthCheck(
        double maximumCpuPercentage,
        TimeSpan? sampleWindow = null,
        ILogger<ProcessCpuHealthCheck>? logger = null)
    {
        _maximumCpuPercentage = maximumCpuPercentage;
        _process = Process.GetCurrentProcess();
        _sampleWindow = sampleWindow ?? TimeSpan.FromSeconds(5);
        _logger = logger;

        // Captura inicial para establecer el punto de comparación (baseline).
        _lastCpuTime = _process.TotalProcessorTime;
        _lastSampleTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Evalúa si el consumo de CPU está dentro de los umbrales de eficiencia esperados.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentCpuTime = _process.TotalProcessorTime;
            var currentSampleTime = DateTime.UtcNow;

            // Diferenciales de tiempo de CPU consumido vs tiempo real transcurrido.
            var cpuTimeDiff = currentCpuTime - _lastCpuTime;
            var timeDiff = currentSampleTime - _lastSampleTime;

            // Cálculo Senior: Normalización por número de procesadores lógicos.
            // Formula: (TiempoCPU / (TiempoReal * Núcleos)) * 100
            var cpuUsedMs = cpuTimeDiff.TotalMilliseconds;
            var totalMsPassed = timeDiff.TotalMilliseconds;
            var cpuUsagePercentage = (cpuUsedMs / (totalMsPassed * Environment.ProcessorCount)) * 100;

            // Persistencia del estado para la siguiente iteración del HealthCheck.
            _lastCpuTime = currentCpuTime;
            _lastSampleTime = currentSampleTime;

            // Manejo de Cold Start: Si no ha pasado suficiente tiempo, forzamos un pequeño delay 
            // para obtener una muestra estadísticamente significativa.
            if (cpuTimeDiff == TimeSpan.Zero || timeDiff < TimeSpan.FromSeconds(1))
            {
                await Task.Delay(1000, cancellationToken);
                return await CheckHealthAsync(context, cancellationToken);
            }

            var data = new Dictionary<string, object>
            {
                { "cpu_usage_percentage", Math.Round(cpuUsagePercentage, 2) },
                { "maximum_allowed_percentage", _maximumCpuPercentage },
                { "processor_count", Environment.ProcessorCount },
                { "total_processor_time_ms", Math.Round(currentCpuTime.TotalMilliseconds, 2) },
                { "os_description", RuntimeInformation.OSDescription },
                { "os_architecture", RuntimeInformation.OSArchitecture.ToString() },
                { "process_architecture", RuntimeInformation.ProcessArchitecture.ToString() },
                { "framework_version", RuntimeInformation.FrameworkDescription },
                { "sample_duration_ms", Math.Round(timeDiff.TotalMilliseconds, 2) }
            };

            // Escenario Crítico: Saturación de CPU (Throttling o Bucles infinitos).
            if (cpuUsagePercentage > _maximumCpuPercentage)
            {
                var message = $"⚠️ CPU CRÍTICO: {cpuUsagePercentage:F2}% (límite: {_maximumCpuPercentage}%)";
                _logger?.LogWarning(message);
                return HealthCheckResult.Unhealthy(message, data: data);
            }

            // Escenario Preventivo: Carga elevada sostenida.
            if (cpuUsagePercentage > _maximumCpuPercentage * 0.8)
            {
                var message = $"⚠️ CPU ALTO: {cpuUsagePercentage:F2}% (Umbral preventivo del 80%)";
                _logger?.LogDebug(message);
                return HealthCheckResult.Degraded(message, data: data);
            }

            return HealthCheckResult.Healthy(
                $"✅ CPU Normal: {cpuUsagePercentage:F2}%",
                data: data);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Fallo en la telemetría de rendimiento de CPU.");

            return HealthCheckResult.Unhealthy(
                "Error al medir uso de CPU",
                ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "error_type", ex.GetType().Name }
                });
        }
    }
}