/// <summary>
/// Define un contrato para la gestión de caché distribuido o en memoria.
/// </summary>
public interface ICacheService
{
    /// <summary> Recupera un objeto del caché. </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary> Almacena un objeto con un tiempo de vida específico. </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary> Elimina una entrada del caché. </summary>
    Task RemoveAsync(string key);
}
