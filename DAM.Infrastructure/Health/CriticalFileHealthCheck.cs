using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

/// <summary>
/// Monitor de salud especializado en la integridad de activos físicos del sistema.
/// Verifica no solo la existencia, sino la consistencia volumétrica de archivos críticos 
/// para prevenir fallos operativos por archivos corruptos o vacíos.
/// </summary>
public class CriticalFileHealthCheck : IHealthCheck
{
    private readonly string _filePath;
    private readonly long _minimumSizeBytes;
    private readonly ILogger<CriticalFileHealthCheck>? _logger;

    /// <param name="filePath">Ruta absoluta o relativa al recurso crítico.</param>
    /// <param name="minimumSizeBytes">Umbral mínimo de seguridad. Archivos por debajo de este peso se consideran sospechosos.</param>
    /// <param name="logger">Opcional: Logger para trazabilidad de errores de I/O.</param>
    public CriticalFileHealthCheck(
        string filePath,
        long minimumSizeBytes,
        ILogger<CriticalFileHealthCheck>? logger = null)
    {
        _filePath = filePath;
        _minimumSizeBytes = minimumSizeBytes;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta la validación de estado. Se utiliza Task.FromResult por ser operaciones de metadatos 
    /// de sistema de archivos usualmente rápidas (non-blocking en la mayoría de OS modernos).
    /// </summary>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fileInfo = new FileInfo(_filePath);

            // Estructura de metadatos enriquecida para facilitar el diagnóstico desde dashboards de monitoreo.
            var data = new Dictionary<string, object>
            {
                { "file_path", _filePath },
                { "exists", fileInfo.Exists },
                { "size_bytes", fileInfo.Length },
                { "size_mb", Math.Round(fileInfo.Length / 1024.0 / 1024.0, 2) },
                { "last_modified", fileInfo.LastWriteTimeUtc },
                { "last_accessed", fileInfo.LastAccessTimeUtc },
                { "is_readonly", fileInfo.IsReadOnly },
                { "directory", fileInfo.DirectoryName ?? "unknown" }
            };

            // Escenario Crítico: La ausencia del archivo impide la ejecución correcta del servicio.
            if (!fileInfo.Exists)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"❌ Archivo crítico no encontrado: {_filePath}",
                    data: data));
            }

            // Escenario de Degradación: El archivo existe pero su tamaño sugiere que es inválido o está truncado.
            // Se marca como 'Degraded' para alertar a soporte sin necesariamente detener el servicio.
            if (fileInfo.Length < _minimumSizeBytes)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"⚠️ Archivo más pequeño de lo esperado: {fileInfo.Length} bytes (mínimo: {_minimumSizeBytes})",
                    data: data));
            }

            // Estado Óptimo.
            return Task.FromResult(HealthCheckResult.Healthy(
                $"✅ Archivo OK: {fileInfo.Name} ({fileInfo.Length:N0} bytes)",
                data: data));
        }
        catch (Exception ex)
        {
            // Captura de excepciones de permisos (UnauthorizedAccess) o bloqueos de I/O.
            _logger?.LogError(ex, "Error de I/O durante el Health Check del archivo: {FilePath}", _filePath);

            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Error verificando archivo: {ex.Message}",
                ex,
                data: new Dictionary<string, object> { { "file_path", _filePath } }));
        }
    }
}