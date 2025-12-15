using DAM.Core.Entities;
using DAM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Infrastructure.Persistence
{
    public class FixedPriceInvoiceCalculator : IInvoiceCalculator
    {
        // Precio fijo por archivo copiado (ficticio)
        private const decimal PRICE_PER_FILE = 0.05m;

        /// <inheritdoc/>
        public Invoice CalculateInvoice(DeviceActivity activity)
        {
            // 1. Obtener el número de archivos copiados.
            // NOTA: FilesCopied es una lista de strings; usamos su Count.
            int filesCount = activity.FilesCopied?.Count ?? 0;

            // 2. Calcular el monto total
            decimal total = filesCount * PRICE_PER_FILE;

            // 3. Crear el objeto Invoice
            return new Invoice
            {
                SerialNumber = activity.SerialNumber,
                Timestamp = DateTime.UtcNow,
                TotalAmount = total,
                Description = $"Factura por {filesCount} archivo(s) copiado(s) a un costo de {PRICE_PER_FILE:C} c/u."
            };
        }
    }
}
