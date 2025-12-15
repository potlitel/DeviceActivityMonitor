using DAM.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Core.Interfaces
{
    public interface IDevicePersistenceService // Nuevo nombre más sugerente
    {
        /// <summary>
        /// Persiste un registro de presencia de dispositivo (conexión) en la base de datos.
        /// Esta operación es asíncrona y transaccional.
        /// </summary>
        Task PersistPresenceAsync(string serialNumber);

        /// <summary>
        /// Almacena una actividad completada de un dispositivo en la base de datos.
        /// Esta operación es asíncrona y transaccional.
        /// </summary>
        Task PersistActivityAsync(DeviceActivity activity);

        /// <summary>
        /// Registra un evento de inicio o parada del Worker Service en el historial de la base de datos.
        /// Se utiliza para auditar el ciclo de vida del servicio principal.
        /// </summary>
        Task PersistServiceEventAsync(ServiceEvent serviceEvent);

        /// <summary>
        /// Calcula una factura basándose en la actividad inicial y la persiste en la base de datos.
        /// Esta operación es asíncrona, transaccional y utiliza el IInvoiceCalculator inyectado.
        /// </summary>
        Task PersistInvoiceAsync(DeviceActivity activity);
    }
}
