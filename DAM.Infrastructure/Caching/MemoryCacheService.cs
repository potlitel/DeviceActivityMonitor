using DAM.Core.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace DAM.Infrastructure.Caching;

/// <summary>
/// 💾 Implementación de <see cref="ICacheService"/> utilizando IMemoryCache de .NET.
/// </summary>
/// <remarks>
/// <para>
/// <b>Características:</b>
/// <list type="bullet">
/// <item><description>✅ **Almacenamiento en memoria RAM** - Ultra rápido</description></item>
/// <item><description>✅ **Expiración automática** - Soporta absoluta y deslizante</description></item>
/// <item><description>✅ **Thread-safe** - Diseñado para entornos multi-hilo</description></item>
/// <item><description>✅ **Compactación automática** - Libera memoria bajo presión</description></item>
/// </list>
/// </para>
/// <para>
/// <b>⚠️ Limitaciones:</b>
/// <list type="bullet">
/// <item><description>❌ **No distribuido** - Cada instancia tiene su propio caché</description></item>
/// <item><description>❌ **Volátil** - Los datos se pierden al reiniciar la aplicación</description></item>
/// <item><description>❌ **Escalamiento horizontal** - No compartido entre réplicas</description></item>
/// </list>
/// Para entornos distribuidos, considere Redis o SQL Server Cache.
/// </para>
/// <para>
/// <b>📊 Rendimiento:</b>
/// | Operación | Complejidad | Tiempo (aprox) |
/// |-----------|-------------|----------------|
/// | GetAsync  | O(1)        | &lt; 1ms |
/// | SetAsync  | O(1)        | &lt; 1ms |
/// | RemoveAsync | O(1)      | &lt; 1ms |
/// </list>
/// </para>
/// </remarks>
public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

    /// <inheritdoc/>
    public Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clave no puede estar vacía", nameof(key));

        var value = _cache.Get<T>(key);

        return Task.FromResult(value);
    }

    /// <inheritdoc/>
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clave no puede estar vacía", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value), "No se puede cachear un valor nulo");

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
            SlidingExpiration = TimeSpan.FromMinutes(2), // Renovar si se accede
            Priority = CacheItemPriority.Normal
        };

        _cache.Set(key, value, options);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clave no puede estar vacía", nameof(key));

        _cache.Remove(key);

        return Task.CompletedTask;
    }
}