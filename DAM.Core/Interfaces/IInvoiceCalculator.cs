using DAM.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Core.Interfaces
{
    public interface IInvoiceCalculator
    {
        /// <summary>
        /// Calcula una factura basándose en la actividad inicial del dispositivo.
        /// </summary>
        /// <param name="activity">La entidad de actividad que contiene datos iniciales (ej: archivos copiados).</param>
        /// <returns>Una nueva entidad Invoice con el cálculo.</returns>
        Invoice CalculateInvoice(DeviceActivity activity);
    }
}
