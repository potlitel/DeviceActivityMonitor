using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Monitor preventivo de seguridad de transporte (TLS/SSL).
/// Valida la vigencia de los certificados SSL para evitar interrupciones de servicio 
/// por expiración de llaves públicas en endpoints críticos.
/// </summary>
public class SslCertificateHealthCheck : IHealthCheck
{
    private readonly string _uri;
    private readonly int _daysUntilExpiry;
    private readonly ILogger<SslCertificateHealthCheck>? _logger;

    /// <param name="uri">URL del servicio HTTPS a inspeccionar.</param>
    /// <param name="daysUntilExpiry">Umbral de días para marcar el estado como 'Degraded'.</param>
    public SslCertificateHealthCheck(
        string uri,
        int daysUntilExpiry,
        ILogger<SslCertificateHealthCheck>? logger = null)
    {
        _uri = uri;
        _daysUntilExpiry = daysUntilExpiry;
        _logger = logger;
    }

    /// <summary>
    /// Realiza una solicitud 'Head' o 'Get' para interceptar el certificado del servidor.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Senior Tip: Usamos un Handler que ignore errores de validación para poder inspeccionar 
            // el certificado incluso si ya está expirado (de lo contrario, GetAsync lanzaría excepción).
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            using var httpClient = new HttpClient(handler);

            // Usamos HttpCompletionOption.ResponseHeadersRead para optimizar el rendimiento: 
            // no necesitamos el cuerpo de la respuesta, solo el handshake TLS.
            using var response = await httpClient.GetAsync(_uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            // Extracción del certificado desde el mensaje de petición (propiedad enriquecida por el framework).
            var cert = response.RequestMessage?.GetHttpRequestMessageCertificate();

            if (cert == null)
            {
                return HealthCheckResult.Unhealthy($"❌ No se pudo recuperar el certificado SSL de: {_uri}");
            }

            var daysUntilExpiration = (cert.NotAfter - DateTime.UtcNow).TotalDays;

            var data = new Dictionary<string, object>
            {
                { "uri", _uri },
                { "issuer", cert.Issuer },
                { "subject", cert.Subject },
                { "not_after", cert.NotAfter },
                { "days_until_expiry", Math.Round(daysUntilExpiration, 1) },
                { "thumbprint", cert.Thumbprint }
            };

            // Escenario 1: Expiración inmediata (Bloqueante).
            if (daysUntilExpiration < 0)
            {
                return HealthCheckResult.Unhealthy($"❌ Certificado EXPIRADO desde hace {Math.Abs(daysUntilExpiration):F1} días", data: data);
            }

            // Escenario 2: Riesgo Crítico (Ventana de renovación fallida).
            if (daysUntilExpiration < 7)
            {
                return HealthCheckResult.Unhealthy($"🚨 Alerta crítica: Expira en menos de una semana ({daysUntilExpiration:F1} días)", data: data);
            }

            // Escenario 3: Advertencia (Umbral configurable).
            if (daysUntilExpiration < _daysUntilExpiry)
            {
                return HealthCheckResult.Degraded($"⚠️ Advertencia: Renovación necesaria pronto ({daysUntilExpiration:F1} días)", data: data);
            }

            return HealthCheckResult.Healthy($"✅ Certificado OK. Válido por {daysUntilExpiration:F1} días", data: data);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Fallo al intentar validar SSL en {Uri}", _uri);
            return HealthCheckResult.Unhealthy($"Error SSL: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Extensiones para facilitar la extracción de certificados en flujos de HttpClient.
/// </summary>
internal static class HttpRequestMessageExtensions
{
    public static X509Certificate2? GetHttpRequestMessageCertificate(this HttpRequestMessage request)
    {
        // En .NET Core/5+, el certificado se encuentra en las propiedades de la petición tras el handshake.
        return request.Options.TryGetValue(new HttpRequestOptionsKey<X509Certificate2>("X509Certificate2"), out var cert)
               ? cert
               : null;
    }
}