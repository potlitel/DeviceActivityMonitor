using DAM.Core.Entities;

namespace DAM.Core.Interfaces
{
    public interface IInvoiceCalculator
    {
        /// <summary>
        /// Calcula una factura basándose en la actividad final del dispositivo.
        /// </summary>
        /// <param name="activity">La entidad de actividad que contiene datos iniciales (ej: archivos copiados).</param>
        /// <returns>Una nueva entidad Invoice con el cálculo.</returns>
        Invoice? CalculateInvoice(DeviceActivity activity);
    }
}
