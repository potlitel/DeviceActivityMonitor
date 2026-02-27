using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Sockets;

/// <summary>
/// Monitor de conectividad a nivel de transporte (Capa 4 OSI).
/// Verifica la disponibilidad de servicios externos mediante intentos de apertura de sockets TCP,
/// permitiendo diagnosticar problemas de firewall, ruteo o caída de servicios remotos.
/// </summary>
public class TcpPortHealthCheck : IHealthCheck
{
    private readonly string _host;
    private readonly int _port;
    private readonly ILogger<TcpPortHealthCheck>? _logger;

    /// <param name="host">Dirección IP o FQDN del servidor destino.</param>
    /// <param name="port">Puerto TCP (ej. 1433 para SQL Server, 6379 para Redis).</param>
    /// <param name="logger">Logger para trazabilidad de fallos de red.</param>
    public TcpPortHealthCheck(string host, int port, ILogger<TcpPortHealthCheck>? logger = null)
    {
        _host = host;
        _port = port;
        _logger = logger;
    }

    /// <summary>
    /// Intenta establecer una conexión TCP asíncrona dentro de una ventana de tiempo definida.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // El uso de Stopwatch permite medir el SLA de conexión, útil para detectar degradación.
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Senior Tip: El bloque 'using' asegura que el socket se cierre y se libere 
            // el file descriptor en el Sistema Operativo, evitando el agotamiento de puertos (port exhaustion).
            using var tcpClient = new TcpClient();

            var connectTask = tcpClient.ConnectAsync(_host, _port);

            // Implementación de Timeout Manual: ConnectAsync no siempre respeta el CancellationToken de forma inmediata.
            // Se utiliza Task.WhenAny para garantizar que el HealthCheck no se quede colgado indefinidamente.
            if (await Task.WhenAny(connectTask, Task.Delay(5000, cancellationToken)) == connectTask)
            {
                await connectTask; // Propagar excepciones si la tarea falló (ej. Connection Refused).
                stopwatch.Stop();

                var data = new Dictionary<string, object>
                {
                    { "host", _host },
                    { "port", _port },
                    { "connection_time_ms", Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2) },
                    { "local_endpoint", tcpClient.Client.LocalEndPoint?.ToString() ?? "unknown" }
                };

                return HealthCheckResult.Healthy($"✅ Puerto {_port} accesible en {_host}", data);
            }
            else
            {
                stopwatch.Stop();
                // Si Task.Delay finalizó primero, tratamos la conexión como fallida por tiempo de espera.
                throw new TimeoutException($"Límite de espera de 5s excedido conectando a {_host}:{_port}");
            }
        }
        catch (Exception ex)
        {
            // Logueamos como Warning ya que los fallos de red suelen ser transitorios (jitter) o externos.
            _logger?.LogWarning(ex, "Fallo de conectividad TCP en {Host}:{Port}", _host, _port);

            return HealthCheckResult.Unhealthy(
                $"❌ No se puede conectar a {_host}:{_port}",
                ex,
                data: new Dictionary<string, object>
                {
                    { "host", _host },
                    { "port", _port },
                    { "error", ex.Message },
                    { "attempt_duration_ms", Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2) }
                });
        }
    }
}