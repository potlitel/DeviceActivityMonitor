namespace DAM.Frontend.Core.Interfaces
{
    /// <summary>
    /// 💾 Contrato para almacenamiento local seguro
    /// </summary>
    public interface IStorageService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value);
        Task RemoveAsync(string key);
        Task ClearAsync();
    }
}
