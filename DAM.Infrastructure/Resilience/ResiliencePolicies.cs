using Polly;
using Polly.Retry;

namespace DAM.Infrastructure.Resilience;

public static class ResiliencePolicies
{
    /// <summary>
    /// Política de reintento exponencial para operaciones de base de datos.
    /// </summary>
    public static AsyncRetryPolicy CreateDbRetryPolicy()
    {
        return Policy
            .Handle<Exception>() // Podrías filtrar por SqliteException
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    // Loguear el reintento si es necesario
                });
    }
}