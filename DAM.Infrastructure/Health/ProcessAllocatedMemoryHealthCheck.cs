using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

/// <summary>
/// Monitor de consumo de recursos de hardware enfocado en la memoria volátil del proceso actual.
/// Esencial para detectar 'Memory Leaks' en servicios de larga duración (BackgroundServices)
/// y prevenir el reinicio abrupto del proceso por parte del sistema operativo.
/// </summary>
public class ProcessAllocatedMemoryHealthCheck : IHealthCheck
{
    private readonly long _maximumMegabytesAllocated;
    private readonly Process _process;

    /// <param name="maximumMegabytesAllocated">Límite superior de memoria (SLA) antes de considerar el servicio en estado crítico.</param>
    public ProcessAllocatedMemoryHealthCheck(long maximumMegabytesAllocated)
    {
        _maximumMegabytesAllocated = maximumMegabytesAllocated;
        // Se cachea la referencia al proceso actual para optimizar las llamadas recurrentes del HealthCheck.
        _process = Process.GetCurrentProcess();
    }

    /// <summary>
    /// Evalúa la presión de memoria del proceso. 
    /// Utiliza 'PrivateMemorySize64' ya que representa la memoria que no puede ser compartida con otros procesos 
    /// y es el indicador más preciso de fugas de memoria en aplicaciones .NET.
    /// </summary>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // El acceso a propiedades de Process refresca los contadores internamente.
        var allocatedMemoryBytes = _process.PrivateMemorySize64;
        var allocatedMemoryMB = allocatedMemoryBytes / 1024.0 / 1024.0;

        // Metadatos para diagnósticos forenses en dashboards de monitoreo.
        var data = new Dictionary<string, object>
        {
            { "allocated_memory_mb", Math.Round(allocatedMemoryMB, 2) },
            { "peak_memory_mb", Math.Round(_process.PeakWorkingSet64 / 1024.0 / 1024.0, 2) },
            { "process_id", _process.Id },
            { "process_name", _process.ProcessName },
            { "maximum_allowed_mb", _maximumMegabytesAllocated },
            { "threads_count", _process.Threads.Count } // Dato extra Senior: monitorear si hay fugas de hilos.
        };

        // Escenario Crítico: Se ha superado el umbral definido. Riesgo inminente de OutOfMemoryException.
        if (allocatedMemoryMB > _maximumMegabytesAllocated)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"❌ Límite de memoria excedido: {allocatedMemoryMB:F2} MB (Límite: {_maximumMegabytesAllocated} MB)",
                data: data));
        }

        // Escenario Preventivo: Umbral del 80%. 
        // Permite a los administradores actuar (ej. reiniciar el servicio de forma controlada) antes del fallo total.
        if (allocatedMemoryMB > _maximumMegabytesAllocated * 0.8)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"⚠️ Presión de memoria alta: {allocatedMemoryMB:F2} MB (80% del límite alcanzado)",
                data: data));
        }

        // Operación dentro de los parámetros normales de diseño.
        return Task.FromResult(HealthCheckResult.Healthy(
            $"✅ Memoria estable: {allocatedMemoryMB:F2} MB",
            data: data));
    }
}