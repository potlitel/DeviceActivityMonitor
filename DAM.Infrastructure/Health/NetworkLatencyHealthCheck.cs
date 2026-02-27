using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;

/// <summary>
/// Monitor de conectividad de red basado en el protocolo ICMP (Ping).
/// Evalúa la calidad del enlace mediante la medición del RTT (Round Trip Time), 
/// permitiendo detectar degradación de red antes de que ocurran timeouts en la capa de aplicación.
/// </summary>
public class NetworkLatencyHealthCheck : IHealthCheck
{
    private readonly string _host;
    private readonly int _timeoutMilliseconds;
    private readonly ILogger<NetworkLatencyHealthCheck>? _logger;

    /// <param name="host">Dirección IP o FQDN del destino a monitorear.</param>
    /// <param name="timeoutMilliseconds">Tiempo máximo de espera para la respuesta ICMP.</param>
    /// <param name="logger">Logger para registrar fallos de red persistentes.</param>
    public NetworkLatencyHealthCheck(
        string host,
        int timeoutMilliseconds,
        ILogger<NetworkLatencyHealthCheck>? logger = null)
    {
        _host = host;
        _timeoutMilliseconds = timeoutMilliseconds;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta la validación de latencia. Utiliza semántica de estados basada en SLAs de red:
    /// - Healthy: Operación normal (Baja latencia).
    /// - Degraded: Conectividad establecida pero con rendimiento pobre (Alta latencia).
    /// - Unhealthy: Pérdida total de comunicación o errores de protocolo.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // El uso de 'using' garantiza la liberación de recursos del sistema operativo asociados al socket ICMP.
            using var ping = new Ping();

            // Operación asíncrona pura para no bloquear los hilos del thread pool durante el tiempo de espera de red.
            var reply = await ping.SendPingAsync(_host, _timeoutMilliseconds);

            var data = new Dictionary<string, object>
            {
                { "host", _host },
                { "roundtrip_time_ms", reply.RoundtripTime },
                { "status", reply.Status.ToString() },
                { "ip_address", reply.Address?.ToString() ?? "unknown" }
            };

            // Lógica de evaluación basada en umbrales de latencia.
            // Nota: Estos valores (100ms/300ms) deberían idealmente venir de configuración externa (IOptions).
            return reply.Status switch
            {
                // Latencia óptima para operaciones en tiempo real o sincronizaciones rápidas.
                IPStatus.Success when reply.RoundtripTime < 100 =>
                    HealthCheckResult.Healthy($"✅ Latencia excelente: {reply.RoundtripTime}ms", data: data),

                // Latencia aceptable pero fuera de los parámetros ideales; posible congestión de red.
                IPStatus.Success when reply.RoundtripTime < 300 =>
                    HealthCheckResult.Healthy($"⚠️ Latencia moderada: {reply.RoundtripTime}ms", data: data),

                // La conexión existe pero es "pobre". Las transacciones de DB o subidas de archivos podrían fallar.
                IPStatus.Success =>
                    HealthCheckResult.Degraded($"🐢 Latencia alta: {reply.RoundtripTime}ms", data: data),

                // Fallo total de respuesta (TimeOut, DestinationUnreachable, etc).
                _ => HealthCheckResult.Unhealthy($"❌ Sin respuesta: {reply.Status}", data: data)
            };
        }
        catch (Exception ex)
        {
            // Fallos catastróficos: errores de resolución DNS o falta de permisos para enviar paquetes ICMP.
            _logger?.LogError(ex, "Excepción crítica durante el ping a {Host}", _host);

            return HealthCheckResult.Unhealthy(
                $"Error en ping a {_host}: {ex.Message}",
                exception: ex,
                data: new Dictionary<string, object> { { "host", _host } });
        }
    }
}