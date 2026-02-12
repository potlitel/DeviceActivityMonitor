namespace DAM.Core.Abstractions;

/// <summary>
/// ⚡ Define un contrato para la gestión de caché distribuido o en memoria.
/// </summary>
/// <remarks>
/// <para>
/// <b>🎯 Propósito:</b>
/// Abstraer la implementación concreta del mecanismo de caché, permitiendo:
/// <list type="bullet">
/// <item><description>✅ **Desacoplar** la lógica de negocio de la infraestructura</description></item>
/// <item><description>✅ **Mejorar rendimiento** reduciendo viajes a BD/APIs</description></item>
/// <item><description>✅ **Implementar** patrones como Cache-Aside y Cache-Penetration</description></item>
/// <item><description>✅ **Estrategias** de expiración configurables</description></item>
/// </list>
/// </para>
/// <para>
/// <b>📌 Estrategias de Caché:</b>
/// | Patrón | Cuándo usarlo |
/// |--------|---------------|
/// | **Cache-Aside** | Lecturas frecuentes de datos que cambian ocasionalmente |
/// | **Write-Through** | Escrituras que deben reflejarse inmediatamente |
/// | **Write-Behind** | Alto volumen de escrituras, consistencia eventual |
/// </list>
/// </para>
/// <para>
/// <b>⚠️ Consideraciones importantes:</b>
/// <list type="bullet">
/// <item><description>🔐 **Seguridad:** No cachear información sensible (contraseñas, tokens)</description></item>
/// <item><description>📏 **Tamaño:** Establecer límites de memoria para evitar DoS</description></item>
/// <item><description>⏰ **Expiración:** Preferir expiración deslizante vs absoluta</description></item>
/// <item><description>🔄 **Invalidación:** Estrategia clara para actualizar/eliminar entradas</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Patrón Cache-Aside con decorador:
/// <code>
/// public class CachedUserRepository : IUserRepository
/// {
///     private readonly IUserRepository _inner;
///     private readonly ICacheService _cache;
///     
///     public async Task&lt;UserDto&gt; GetByIdAsync(Guid id)
///     {
///         var key = $"user:{id}";
///         
///         // 1. Intentar obtener del caché
///         var cached = await _cache.GetAsync&lt;UserDto&gt;(key);
///         if (cached != null) return cached;
///         
///         // 2. Fallback a BD
///         var user = await _inner.GetByIdAsync(id);
///         
///         // 3. Almacenar en caché
///         await _cache.SetAsync(key, user, TimeSpan.FromMinutes(15));
///         
///         return user;
///     }
/// }
/// </code>
/// </example>
public interface ICacheService
{
    /// <summary>
    /// 🔍 Recupera un objeto del caché por su clave.
    /// </summary>
    /// <typeparam name="T">Tipo del objeto almacenado.</typeparam>
    /// <param name="key">Clave única del elemento (case-sensitive).</param>
    /// <returns>
    /// El objeto deserializado si existe; 
    /// <see langword="null"/> si no existe o expiró.
    /// </returns>
    /// <remarks>
    /// **Convención de nombres:** Usar formato `{recurso}:{identificador}`
    /// Ejemplos: `user:123`, `activity:456`, `invoice:789`
    /// </remarks>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// 💾 Almacena un objeto en el caché con tiempo de vida opcional.
    /// </summary>
    /// <typeparam name="T">Tipo del objeto a almacenar.</typeparam>
    /// <param name="key">Clave única del elemento.</param>
    /// <param name="value">Objeto a serializar y almacenar.</param>
    /// <param name="expiration">
    /// Tiempo de vida del elemento. 
    /// Si es <see langword="null"/>, se usa el valor por defecto (10 minutos).
    /// </param>
    /// <remarks>
    /// **Estrategias de expiración recomendadas:**
    /// <list type="bullet">
    /// <item><description>⏱️ **Datos maestros:** 1-4 horas</description></item>
    /// <item><description>📊 **Consultas frecuentes:** 5-15 minutos</description></item>
    /// <item><description>🔄 **Tokens/OTP:** 1-5 minutos</description></item>
    /// <item><description>❌ **Nunca usar** expiración infinita en producción</description></item>
    /// </list>
    /// </remarks>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// 🗑️ Elimina una entrada específica del caché.
    /// </summary>
    /// <param name="key">Clave del elemento a eliminar.</param>
    /// <remarks>
    /// **Usar este método cuando:**
    /// - Un recurso es actualizado (invalidación)
    /// - Un recurso es eliminado
    /// - Se necesita forzar una recarga fresca de datos
    /// </remarks>
    Task RemoveAsync(string key);
}