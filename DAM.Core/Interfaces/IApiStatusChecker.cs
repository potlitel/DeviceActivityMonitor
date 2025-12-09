namespace DAM.Core.Interfaces
{
    /// <summary>
    /// Contrato para la verificación del estado de disponibilidad de la Web API remota.
    /// </summary>
    /// <remarks>
    /// Se utiliza para implementar la lógica de Circuit Breaker y la estrategia de resiliencia.
    /// </remarks>
    public interface IApiStatusChecker
    {
        /// <summary>
        /// Verifica de forma asíncrona si la Web API está disponible y responde a las peticiones.
        /// </summary>
        /// <returns>True si la API está disponible y responde con éxito; False en caso contrario.</returns>
        Task<bool> IsApiAvailableAsync();
    }
}
