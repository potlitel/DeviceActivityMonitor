using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DAM.Infrastructure.Health;

/// <summary>
/// Monitor de disponibilidad de almacenamiento persistente.
/// Verifica el espacio libre en la unidad donde se ejecuta la aplicación para prevenir 
/// fallos críticos en la escritura de logs, bases de datos locales o archivos temporales.
/// </summary>
public class StorageHealthCheck : IHealthCheck
{
    private readonly long _unhealthyThresholdBytes;
    private readonly long _degradedThresholdBytes;

    /// <param name="unhealthyMb">Umbral crítico en MB (por defecto 500MB).</param>
    /// <param name="degradedMb">Umbral de advertencia en MB (por defecto 2000MB).</param>
    public StorageHealthCheck(long unhealthyMb = 500, long degradedMb = 2000)
    {
        _unhealthyThresholdBytes = unhealthyMb * 1024 * 1024;
        _degradedThresholdBytes = degradedMb * 1024 * 1024;
    }

    /// <summary>
    /// Analiza el espacio disponible en la partición raíz del servicio.
    /// </summary>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            // Senior Tip: Obtenemos la raíz de ejecución de forma dinámica para adaptarnos a diferentes entornos de despliegue.
            var driveRoot = Path.GetPathRoot(Directory.GetCurrentDirectory())
                            ?? throw new InvalidOperationException("No se pudo determinar la unidad raíz.");

            var drive = new DriveInfo(driveRoot);

            double freeSpaceGb = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
            double totalSizeGb = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
            double percentFree = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;

            // Datos enriquecidos para visualización en dashboards (Grafana/Kibana).
            var data = new Dictionary<string, object>
            {
                { "drive_name", drive.Name },
                { "drive_type", drive.DriveType.ToString() },
                { "drive_format", drive.DriveFormat },
                { "total_size_gb", Math.Round(totalSizeGb, 2) },
                { "free_space_gb", Math.Round(freeSpaceGb, 2) },
                { "free_space_percent", Math.Round(percentFree, 1) },
                { "label", drive.VolumeLabel }
            };

            // Escenario Crítico: Menos del umbral mínimo definido (500MB por defecto).
            if (drive.AvailableFreeSpace < _unhealthyThresholdBytes)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"❌ Espacio crítico: {freeSpaceGb:F2} GB disponibles en {drive.Name}", data: data));
            }

            // Escenario de Advertencia: Menos del umbral preventivo (2GB por defecto).
            if (drive.AvailableFreeSpace < _degradedThresholdBytes)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"⚠️ Espacio bajo: {freeSpaceGb:F2} GB ({percentFree:F1}%) en {drive.Name}", data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"✅ Almacenamiento óptimo: {freeSpaceGb:F2} GB libres.", data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Error accediendo a métricas de almacenamiento: {ex.Message}", ex));
        }
    }
}