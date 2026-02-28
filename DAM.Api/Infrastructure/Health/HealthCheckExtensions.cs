using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace DAM.Api.Infrastructure.Health;

public static class HealthCheckExtensions
{
    public static HealthCheckOptions GetJsonOptions()
    {
        return new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    Status = report.Status.ToString(),
                    Duration = report.TotalDuration,
                    Checks = report.Entries.Select(e => new
                    {
                        Component = e.Key,
                        Status = e.Value.Status.ToString(),
                        Description = e.Value.Description,
                        Duration = e.Value.Duration
                    })
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        };
    }

    // Este es el método que estabas buscando
    public static IHealthChecksBuilder AddProcessAllocatedMemoryCheck(
       this IHealthChecksBuilder builder,
       long maximumMegabytesAllocated,
       string? name = null,
       HealthStatus? failureStatus = null,
       IEnumerable<string>? tags = null)
    {
        // 👇 IMPORTANTE: Usamos AddCheck con factory para pasar el parámetro
        return builder.Add(new HealthCheckRegistration(
            name ?? "process_allocated_memory",
            sp => new ProcessAllocatedMemoryHealthCheck(maximumMegabytesAllocated), // 👈 Pasamos el parámetro
            failureStatus ?? HealthStatus.Unhealthy,
            tags ?? new[] { "memory", "process", "system" }
        ));
    }

    // También puedes crear otros métodos útiles
    public static IHealthChecksBuilder AddProcessCpuCheck(
        this IHealthChecksBuilder builder,
        double maximumCpuPercentage,
        TimeSpan? sampleWindow = null,
        string? name = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? "process_cpu",
            sp => new ProcessCpuHealthCheck(
                maximumCpuPercentage,
                sampleWindow ?? TimeSpan.FromSeconds(5),
                sp.GetService<ILogger<ProcessCpuHealthCheck>>()),
            failureStatus ?? HealthStatus.Unhealthy,
            tags ?? new[] { "cpu", "process", "performance" }
        ));
    }

    // 🆕 NUEVO: Health Check para puertos de red específicos
    public static IHealthChecksBuilder AddTcpPortHealthCheck(
        this IHealthChecksBuilder builder,
        string host,
        int port,
        string? name = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? $"tcp_port_{port}",
            sp => new TcpPortHealthCheck(host, port, sp.GetService<ILogger<TcpPortHealthCheck>>()),
            failureStatus ?? HealthStatus.Unhealthy,
            tags ?? new[] { "network", "tcp", "connectivity" }
        ));
    }

    // 🆕 NUEVO: Health Check para latencia de red
    public static IHealthChecksBuilder AddNetworkLatencyCheck(
       this IHealthChecksBuilder builder,
       string host,
       int timeoutMilliseconds = 5000,
       string? name = null,
       HealthStatus? failureStatus = null,
       IEnumerable<string>? tags = null)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? $"latency_{host}",
            sp => new NetworkLatencyHealthCheck(host, timeoutMilliseconds, sp.GetService<ILogger<NetworkLatencyHealthCheck>>()),
            failureStatus ?? HealthStatus.Unhealthy,
            tags ?? new[] { "network", "latency", "connectivity" }
        ));
    }

    // 🆕 NUEVO: Health Check para certificados SSL
    public static IHealthChecksBuilder AddSslCertificateCheck(
        this IHealthChecksBuilder builder,
        string uri,
        int daysUntilExpiry = 30,
        string? name = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? $"ssl_{new Uri(uri).Host}",
            sp => new SslCertificateHealthCheck(uri, daysUntilExpiry, sp.GetService<ILogger<SslCertificateHealthCheck>>()),
            failureStatus ?? HealthStatus.Unhealthy,
            tags ?? new[] { "ssl", "security", "certificate" }
        ));
    }

    // 🆕 NUEVO: Health Check para archivos críticos
    public static IHealthChecksBuilder AddCriticalFileCheck(
        this IHealthChecksBuilder builder,
        string filePath,
        long minimumSizeBytes = 0,
        string? name = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? $"file_{Path.GetFileName(filePath)}",
            sp => new CriticalFileHealthCheck(filePath, minimumSizeBytes, sp.GetService<ILogger<CriticalFileHealthCheck>>()),
            failureStatus ?? HealthStatus.Unhealthy,
            tags ?? new[] { "file", "storage", "critical" }
        ));
    }

    // 🆕 NUEVO: Health Check para dispositivos externos (tu dominio)
    //public static IHealthChecksBuilder AddExternalDevicesHealthCheck(
    //    this IHealthChecksBuilder builder,
    //    TimeSpan inactiveThreshold,
    //    string? name = null,
    //    HealthStatus? failureStatus = null,
    //    IEnumerable<string>? tags = null)
    //{
    //    return builder.AddCheck<ExternalDevicesHealthCheck>(
    //        name ?? "external_devices",
    //        failureStatus ?? HealthStatus.Degraded,
    //        tags ?? new[] { "domain", "devices", "business" });
    //}

    // 🆕 NUEVO: Health Check para temperatura del sistema (Windows)
    public static IHealthChecksBuilder AddSystemTemperatureCheck(
        this IHealthChecksBuilder builder,
        double maximumTemperatureCelsius = 80.0,
        string? name = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? "system_temperature",
            sp => new SystemTemperatureHealthCheck(maximumTemperatureCelsius, sp.GetService<ILogger<SystemTemperatureHealthCheck>>()),
            failureStatus ?? HealthStatus.Unhealthy,
            tags ?? new[] { "hardware", "temperature", "system" }
        ));
    }
}