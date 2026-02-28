using DAM.Api.Base;
using DAM.Core.Common;
using FastEndpoints;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

/// <summary>
/// Representa el estado detallado de un componente del sistema
/// </summary>
public record ComponentHealthDetail(
    string Name,                    // Nombre del componente con emoji
    string Status,                   // Healthy/Degraded/Unhealthy
    string StatusEmoji,              // ✅/⚠️/❌ para visualización
    string Duration,                  // Tiempo de ejecución
    string Description,               // Descripción principal
    IReadOnlyDictionary<string, object> Metadata,  // Datos adicionales (temperatura, %, etc)
    string[] Tags,                     // Etiquetas de categorización
    string? ExceptionMessage           // Mensaje de error si existe
);

/// <summary>
/// Resumen ejecutivo del estado del sistema
/// </summary>
public record HealthSummary(
    int TotalComponents,
    int HealthyCount,
    int DegradedCount,
    int UnhealthyCount,
    double HealthPercentage,
    string OverallStatusEmoji
);

/// <summary>
/// Información detallada del sistema y entorno
/// </summary>
public record SystemEnvironmentInfo(
    string MachineName,
    string OSDescription,
    string OSArchitecture,
    string ProcessArchitecture,
    string FrameworkVersion,
    int ProcessorCount,
    string WorkingDirectory,
    string UserName,
    string UserDomainName,
    string Uptime,
    DateTime ServerTime,
    string TimeZone,
    string HostName,
    string[] IpAddresses,
    long WorkingSetMemory,
    long GCTotalMemory,
    int ThreadCount
);

/// <summary>
/// Respuesta completa del health check unificado
/// </summary>
public record UnifiedHealthResponse(
    string OverallStatus,
    string OverallStatusEmoji,
    DateTime CheckedAt,
    TimeSpan TotalDuration,
    HealthSummary Summary,
    SystemEnvironmentInfo Environment,
    IEnumerable<ComponentHealthDetail> Components,
    Dictionary<string, IEnumerable<ComponentHealthDetail>> ComponentsByTag,
    string[] Recommendations
);

/// <summary>
/// Endpoint unificado que muestra TODA la información de health checks
/// con formato profesional y documentación exhaustiva
/// </summary>
public class UnifiedHealthEndpoint : BaseEndpoint<EmptyRequest, UnifiedHealthResponse>
{
    private readonly HealthCheckService _healthService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UnifiedHealthEndpoint> _logger;

    public UnifiedHealthEndpoint(
        HealthCheckService healthService,
        IWebHostEnvironment environment,
        ILogger<UnifiedHealthEndpoint> logger)
    {
        _healthService = healthService;
        _environment = environment;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/health/unified");
        Get("/health"); // También disponible en la raíz de health
        AllowAnonymous();

        Description(x => x
            .WithTags("🖥️ Sistema", "🔬 Monitoreo", "⭐ Crítico")
            .Produces<UnifiedHealthResponse>(200, "application/json")
            //.Produces(503, "application/json")
            .ProducesProblem(500)
            .WithName("GetUnifiedHealth")
        );

        Summary(s => {
            s.Summary = "🏥 [HEALTH] Panel de control unificado del sistema";
            s.Description = @"
                ═══════════════════════════════════════════════════════════════════════════════
                🏥 **PANEL DE CONTROL DE SALUD DEL SISTEMA**
                ═══════════════════════════════════════════════════════════════════════════════
                
                Este endpoint ejecuta **TODOS** los health checks registrados y proporciona una
                vista completa del estado del sistema, incluyendo:
                
                📊 **RESUMEN EJECUTIVO**
                • Estado global del sistema
                • Conteo de componentes por estado (Healthy/Degraded/Unhealthy)
                • Porcentaje de salud general
                
                🔧 **COMPONENTES MONITOREADOS**
                " + GetMonitoredComponentsList() + @"
                
                💻 **INFORMACIÓN DEL ENTORNO**
                • Sistema operativo y arquitectura
                • Tiempo de actividad (uptime)
                • Memoria y threads
                • Red y conectividad
                
                📌 **RECOMENDACIONES**
                • Sugerencias automáticas basadas en el estado actual
                • Alertas preventivas
                
                ═══════════════════════════════════════════════════════════════════════════════
                **CÓDIGOS DE ESTADO:**
                • 200 OK → Todos los sistemas saludables
                • 503 Service Unavailable → Uno o más componentes degradados o no saludables
                ═══════════════════════════════════════════════════════════════════════════════
            ";

            // Ejemplo de respuesta exitosa
            s.ResponseExamples[200] = GetExampleHealthyResponse();

            // Ejemplo de respuesta con problemas
            s.ResponseExamples[503] = GetExampleDegradedResponse();
        });
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Ejecutar TODOS los health checks registrados
            var report = await _healthService.CheckHealthAsync(ct);

            stopwatch.Stop();

            // Procesar componentes con metadata enriquecida
            var components = ProcessComponents(report.Entries);

            // Generar resumen ejecutivo
            var summary = GenerateSummary(components);

            // Obtener información del entorno
            var environmentInfo = GetEnvironmentInfo();

            // Agrupar componentes por tags
            var componentsByTag = components
                .GroupBy(c => c.Tags.FirstOrDefault() ?? "otros")
                .ToDictionary(
                    g => g.Key,
                    g => g.AsEnumerable()
                );

            // Generar recomendaciones automáticas
            var recommendations = GenerateRecommendations(components, report.Status);

            var response = new UnifiedHealthResponse(
                OverallStatus: report.Status.ToString(),
                OverallStatusEmoji: GetStatusEmoji(report.Status),
                CheckedAt: DateTime.UtcNow,
                TotalDuration: stopwatch.Elapsed,
                Summary: summary,
                Environment: environmentInfo,
                Components: components,
                ComponentsByTag: componentsByTag,
                Recommendations: recommendations
            );

            // Usar el código HTTP apropiado según el estado
            int statusCode = report.Status == HealthStatus.Healthy ? 200 : 503;

            // Mensaje personalizado según el estado
            string message = report.Status switch
            {
                HealthStatus.Healthy => "✅ SISTEMA OPERATIVO - Todos los componentes funcionando correctamente",
                HealthStatus.Degraded => "⚠️ SISTEMA DEGRADADO - Algunos componentes presentan rendimiento reducido",
                HealthStatus.Unhealthy => "❌ SISTEMA CRÍTICO - Componentes esenciales no están funcionando",
                _ => "Estado del sistema"
            };

            await SendSuccessAsync(response, message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al ejecutar health checks");

            var errors = new List<string>
            {
                "Error interno al evaluar la salud del sistema",
                $"Detalle técnico: {ex.Message}"
            };

            var errorResponse = ApiResponse<UnifiedHealthResponse>.Failure(
                errors,
                "No se pudo completar la evaluación de salud"
            );

            await Send.ResultAsync(Results.Json(errorResponse, statusCode: 500));
        }
    }

    #region Métodos de ayuda

    private IEnumerable<ComponentHealthDetail> ProcessComponents(IReadOnlyDictionary<string, HealthReportEntry> entries)
    {
        return entries.Select(e => new ComponentHealthDetail(
            Name: e.Key,
            Status: e.Value.Status.ToString(),
            StatusEmoji: GetStatusEmoji(e.Value.Status),
            Duration: FormatDuration(e.Value.Duration),
            Description: GetEnhancedDescription(e.Key, e.Value),
            Metadata: e.Value.Data ?? new Dictionary<string, object>(),
            Tags: e.Value.Tags.ToArray(),
            ExceptionMessage: e.Value.Exception?.Message
        ));
    }

    private string GetEnhancedDescription(string componentName, HealthReportEntry entry)
    {
        if (!string.IsNullOrEmpty(entry.Description))
            return entry.Description;

        if (entry.Data?.Any() == true)
        {
            return componentName switch
            {
                string name when name.Contains("Memoria") =>
                    $"📊 {GetMetadataValue(entry.Data, "allocated_memory_mb")} MB / 512 MB",

                string name when name.Contains("CPU") =>
                    $"⚡ {GetMetadataValue(entry.Data, "cpu_usage_percentage")}% / 80%",

                string name when name.Contains("Temperatura") =>
                    $"🌡️ {GetMetadataValue(entry.Data, "temperature_celsius")}°C",

                string name when name.Contains("Latencia") =>
                    $"📡 {GetMetadataValue(entry.Data, "roundtrip_time_ms")} ms",

                string name when name.Contains("Almacenamiento") =>
                    $"💾 {GetMetadataValue(entry.Data, "free_space_gb")} GB libres",

                _ => $"{entry.Status} - {entry.Data.FirstOrDefault().Value ?? "Operativo"}"
            };
        }

        return entry.Status switch
        {
            HealthStatus.Healthy => "Funcionando correctamente",
            HealthStatus.Degraded => "Rendimiento reducido",
            HealthStatus.Unhealthy => "No disponible",
            _ => "Estado desconocido"
        };
    }

    private string GetMetadataValue(IReadOnlyDictionary<string, object> data, string key)
    {
        return data.TryGetValue(key, out var value) ? value?.ToString() ?? "N/A" : "N/A";
    }

    private HealthSummary GenerateSummary(IEnumerable<ComponentHealthDetail> components)
    {
        var componentsList = components.ToList();
        var total = componentsList.Count;
        var healthy = componentsList.Count(c => c.Status == "Healthy");
        var degraded = componentsList.Count(c => c.Status == "Degraded");
        var unhealthy = componentsList.Count(c => c.Status == "Unhealthy");

        var healthPercentage = total > 0
            ? Math.Round((healthy + (degraded * 0.5)) / total * 100, 1)
            : 100;

        string overallEmoji = healthPercentage >= 90 ? "🟢" :
                              healthPercentage >= 70 ? "🟡" : "🔴";

        return new HealthSummary(
            TotalComponents: total,
            HealthyCount: healthy,
            DegradedCount: degraded,
            UnhealthyCount: unhealthy,
            HealthPercentage: healthPercentage,
            OverallStatusEmoji: overallEmoji
        );
    }

    private SystemEnvironmentInfo GetEnvironmentInfo()
    {
        var process = Process.GetCurrentProcess();
        var hostName = System.Net.Dns.GetHostName();
        var ipAddresses = System.Net.Dns.GetHostAddresses(hostName)
            .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .Select(ip => ip.ToString())
            .ToArray();

        var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();

        return new SystemEnvironmentInfo(
            MachineName: Environment.MachineName,
            OSDescription: RuntimeInformation.OSDescription,
            OSArchitecture: RuntimeInformation.OSArchitecture.ToString(),
            ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString(),
            FrameworkVersion: RuntimeInformation.FrameworkDescription,
            ProcessorCount: Environment.ProcessorCount,
            WorkingDirectory: Environment.CurrentDirectory,
            UserName: Environment.UserName,
            UserDomainName: Environment.UserDomainName,
            Uptime: $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s",
            ServerTime: DateTime.UtcNow,
            TimeZone: TimeZoneInfo.Local.DisplayName,
            HostName: hostName,
            IpAddresses: ipAddresses,
            WorkingSetMemory: process.WorkingSet64,
            GCTotalMemory: GC.GetTotalMemory(false),
            ThreadCount: process.Threads.Count
        );
    }

    private string[] GenerateRecommendations(IEnumerable<ComponentHealthDetail> components, HealthStatus overallStatus)
    {
        var recommendations = new List<string>();
        var unhealthyComponents = components.Where(c => c.Status == "Unhealthy");
        var degradedComponents = components.Where(c => c.Status == "Degraded");

        if (overallStatus == HealthStatus.Healthy)
        {
            recommendations.Add("✅ Sistema en óptimas condiciones");
            recommendations.Add("📊 Monitoreo regular recomendado cada 5 minutos");
        }
        else
        {
            if (unhealthyComponents.Any())
            {
                recommendations.Add($"❌ ACCIÓN INMEDIATA REQUERIDA: {unhealthyComponents.Count()} componentes no saludables");
                foreach (var comp in unhealthyComponents)
                {
                    recommendations.Add($"   • {comp.Name}: {comp.Description}");
                }
            }

            if (degradedComponents.Any())
            {
                recommendations.Add($"⚠️ REVISAR: {degradedComponents.Count()} componentes degradados");
                foreach (var comp in degradedComponents.Take(3))
                {
                    recommendations.Add($"   • {comp.Name}: {comp.Description}");
                }
            }

            // Recomendaciones específicas por tipo
            if (components.Any(c => c.Name.Contains("Memoria") && c.Status != "Healthy"))
                recommendations.Add("🔧 Considerar aumentar la memoria disponible o revisar fugas de memoria");

            if (components.Any(c => c.Name.Contains("CPU") && c.Status != "Healthy"))
                recommendations.Add("⚡ Alto uso de CPU - Revisar procesos y posible optimización");

            if (components.Any(c => c.Name.Contains("Almacenamiento") && c.Status != "Healthy"))
                recommendations.Add("💾 Espacio de disco bajo - Limpiar archivos temporales o ampliar almacenamiento");
        }

        return recommendations.ToArray();
    }

    private string GetStatusEmoji(HealthStatus status) => status switch
    {
        HealthStatus.Healthy => "✅",
        HealthStatus.Degraded => "⚠️",
        HealthStatus.Unhealthy => "❌",
        _ => "❓"
    };

    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalMilliseconds < 1)
            return "< 1ms";
        if (duration.TotalMilliseconds < 1000)
            return $"{duration.TotalMilliseconds:F1}ms";
        return $"{duration.TotalSeconds:F2}s";
    }

    private string GetMonitoredComponentsList()
    {
        return @"
                • 🧠 Memoria RAM del Proceso
                • ⚡ CPU del Proceso
                • 💾 Almacenamiento en disco
                • 🗄️ Base de Datos Principal
                • 🔌 Puerto SQL Server
                • 📡 Latencia a Internet
                • 🔐 Certificado SSL
                • 📄 Archivo de Configuración
                • 🌡️ Temperatura del Sistema
                • 📱 Dispositivos Externos (próximamente)";
    }

    private UnifiedHealthResponse GetExampleHealthyResponse()
    {
        return new UnifiedHealthResponse(
            OverallStatus: "Healthy",
            OverallStatusEmoji: "✅",
            CheckedAt: DateTime.UtcNow,
            TotalDuration: TimeSpan.FromMilliseconds(245.67),
            Summary: new HealthSummary(9, 8, 1, 0, 94.4, "🟢"),
            Environment: new SystemEnvironmentInfo(
                "SRV-DAM-01",
                "Microsoft Windows 10.0.19045",
                "X64",
                "X64",
                ".NET 8.0.0",
                8,
                "C:\\App\\",
                "svc_dam",
                "DOMAIN",
                "5d 02:30:15",
                DateTime.UtcNow,
                "(UTC-05:00) Eastern Time",
                "srv-dam-01.domain.com",
                new[] { "192.168.1.100", "10.0.0.50" },
                524288000,
                256000000,
                32
            ),
            Components: new List<ComponentHealthDetail>(),
            ComponentsByTag: new Dictionary<string, IEnumerable<ComponentHealthDetail>>(),
            Recommendations: new[] { "✅ Sistema en óptimas condiciones", "📊 Monitoreo regular recomendado cada 5 minutos" }
        );
    }

    private UnifiedHealthResponse GetExampleDegradedResponse()
    {
        return new UnifiedHealthResponse(
            OverallStatus: "Degraded",
            OverallStatusEmoji: "⚠️",
            CheckedAt: DateTime.UtcNow,
            TotalDuration: TimeSpan.FromMilliseconds(345.89),
            Summary: new HealthSummary(9, 6, 2, 1, 72.2, "🟡"),
            Environment: new SystemEnvironmentInfo(
                "SRV-DAM-01",
                "Microsoft Windows 10.0.19045",
                "X64",
                "X64",
                ".NET 8.0.0",
                8,
                "C:\\App\\",
                "svc_dam",
                "DOMAIN",
                "5d 02:30:15",
                DateTime.UtcNow,
                "(UTC-05:00) Eastern Time",
                "srv-dam-01.domain.com",
                new[] { "192.168.1.100", "10.0.0.50" },
                524288000,
                256000000,
                32
            ),
            Components: new List<ComponentHealthDetail>(),
            ComponentsByTag: new Dictionary<string, IEnumerable<ComponentHealthDetail>>(),
            Recommendations: new[]
            {
                "⚠️ REVISAR: 2 componentes degradados",
                "   • 💾 Almacenamiento: Solo 1.2 GB libres",
                "   • 🌡️ Temperatura Sistema: 78°C",
                "❌ ACCIÓN INMEDIATA: 1 componente no saludable",
                "   • 📡 Latencia a Internet: Timeout"
            }
        );
    }

    #endregion
}
