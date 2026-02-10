using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DAM.Infrastructure.Health;

public class StorageHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        var drive = new DriveInfo(Path.GetPathRoot(Directory.GetCurrentDirectory())!);
        double freeSpaceGb = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;

        if (freeSpaceGb < 0.5) // Menos de 500MB es crítico
            return Task.FromResult(HealthCheckResult.Unhealthy($"Espacio en disco insuficiente: {freeSpaceGb:F2} GB"));

        if (freeSpaceGb < 2.0) // Menos de 2GB es advertencia
            return Task.FromResult(HealthCheckResult.Degraded($"Espacio en disco bajo: {freeSpaceGb:F2} GB"));

        return Task.FromResult(HealthCheckResult.Healthy($"Espacio en disco óptimo: {freeSpaceGb:F2} GB"));
    }
}