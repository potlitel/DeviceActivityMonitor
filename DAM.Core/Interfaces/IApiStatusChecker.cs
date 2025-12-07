namespace DAM.Core.Interfaces
{
    // Abstracción para verificar si la API está viva.
    public interface IApiStatusChecker
    {
        Task<bool> IsApiAvailableAsync();
    }
}
