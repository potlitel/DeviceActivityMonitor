using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

/// <summary>
/// Monitor de salud de hardware enfocado en la telemetría térmica.
/// Esencial para prevenir fallos físicos o 'throttling' de CPU en entornos críticos 
/// donde el servicio de Windows opera de forma continua.
/// </summary>
public class SystemTemperatureHealthCheck : IHealthCheck
{
    private readonly double _maximumTemperatureCelsius;
    private readonly ILogger<SystemTemperatureHealthCheck>? _logger;

    public SystemTemperatureHealthCheck(
        double maximumTemperatureCelsius,
        ILogger<SystemTemperatureHealthCheck>? logger = null)
    {
        _maximumTemperatureCelsius = maximumTemperatureCelsius;
        _logger = logger;
    }

    /// <summary>
    /// Evalúa el estado térmico del sistema. Se utiliza Task.FromResult dado que, 
    /// en esta implementación, la obtención de datos es síncrona y no bloqueante.
    /// </summary>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Validación de interoperabilidad: El monitoreo de hardware basado en registros
        // de bajo nivel suele ser dependiente de la plataforma (OS-Specific).
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Task.FromResult(HealthCheckResult.Healthy(
                "✅ Temperatura: No disponible en este SO",
                data: new Dictionary<string, object>
                {
                    { "os", RuntimeInformation.OSDescription },
                    { "note", "Temperature monitoring only available on Windows" }
                }));
        }

        try
        {
            var temperature = GetWindowsCpuTemperature();

            var data = new Dictionary<string, object>
            {
                { "temperature_celsius", temperature },
                { "maximum_allowed_celsius", _maximumTemperatureCelsius },
                { "timestamp", DateTime.UtcNow }
            };

            // Escenario Crítico: Superación del umbral de seguridad térmica definido para el hardware.
            if (temperature > _maximumTemperatureCelsius)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"🔥 Temperatura CRÍTICA: {temperature}°C",
                    data: data));
            }

            // Escenario de Degradación: Margen de seguridad del 10%. 
            // Indica una posible anomalía en la refrigeración antes de llegar al punto crítico.
            if (temperature > _maximumTemperatureCelsius * 0.9) // 90% del límite
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"🌡️ Temperatura ALTA: {temperature}°C",
                    data: data));
            }

            // Estado operativo óptimo.
            return Task.FromResult(HealthCheckResult.Healthy(
                $"❄️ Temperatura normal: {temperature}°C",
                data: data));
        }
        catch (Exception ex)
        {
            // Se opta por LogDebug para no saturar los logs en hardware que no expone sensores a nivel de usuario.
            _logger?.LogDebug(ex, "No se pudo obtener temperatura (puede no estar disponible)");

            // Un fallo en el sensor no debe marcar el servicio como Unhealthy, 
            // ya que la aplicación sigue siendo funcional.
            return Task.FromResult(HealthCheckResult.Healthy(
                "✅ Temperatura: No disponible o sin sensor",
                data: new Dictionary<string, object> { { "note", "Temperature sensor not available" } }));
        }
    }

    /// <summary>
    /// Punto de extensión para la obtención de métricas reales de hardware.
    /// Senior Note: En producción, esta lógica debe consultar namespaces como 'root\WMI' 
    /// (ej. MSAcpi_ThermalZoneTemperature) para obtener datos reales del BIOS/Firmware.
    /// </summary>
    private double GetWindowsCpuTemperature()
    {
        // Implementación simplificada - en producción usar WMI
        // Retorna temperatura simulada para el ejemplo
        return 45.0 + (Random.Shared.NextDouble() * 10);
    }
}