namespace DAM.Core.Settings
{
    /// <summary>
    /// Define los parámetros de comportamiento para las estrategias de persistencia y 
    /// la resiliencia de la comunicación con servicios externos.
    /// </summary>
    public class StorageSettings
    {
        /// <summary>
        /// Obtiene o establece el intervalo de tiempo para reevaluar la disponibilidad de la Web API.
        /// </summary>
        /// <remarks>
        /// Cuando el sistema entra en modo de almacenamiento local (Fallo de API), este valor determina 
        /// cada cuánto tiempo se intentará reconectar con el servicio remoto.
        /// </remarks>
        public int RecheckIntervalMinutes { get; set; }

        /// <summary>
        /// Obtiene o establece el tiempo máximo de espera para las peticiones hacia la API antes de considerar un fallo.
        /// </summary>
        /// <value>
        /// Tiempo en segundos. Un valor demasiado alto puede bloquear hilos de ejecución, 
        /// mientras que uno muy bajo puede causar falsos negativos en redes lentas.
        /// </value>
        public int ApiTimeoutSeconds { get; set; }
    }
}
