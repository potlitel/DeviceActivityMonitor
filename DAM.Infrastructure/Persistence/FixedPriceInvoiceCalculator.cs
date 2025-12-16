using DAM.Core.Entities;
using DAM.Core.Interfaces;

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
            //int filesCount = activity.FilesCopied?.Count ?? 0;

            var copiedFiles = activity.FilesCopied ?? [];
            var deletedFiles = activity.FilesDeleted ?? [];

            // La lista de archivos copiados que sobrevivieron (no fueron eliminados)
            var survivingFiles = copiedFiles.Except(deletedFiles).ToList();

            if (survivingFiles.Count > 0)
            {
                // Número de archivos por los que realmente se cobra
                int filesToBillCount = survivingFiles.Count;

                decimal total = filesToBillCount * PRICE_PER_FILE;

                // La descripción debe reflejar el cálculo
                return new Invoice
                {
                    SerialNumber = activity.SerialNumber,
                    // Usamos ExtractedAt, pues la factura se genera al finalizar la actividad
                    Timestamp = activity.ExtractedAt ?? DateTime.UtcNow,
                    TotalAmount = total,
                    Description = $"Factura por {filesToBillCount} archivo(s) neto(s) (Copiados: {copiedFiles.Count} - Eliminados: {deletedFiles.Count}). Costo: {PRICE_PER_FILE:C} c/u."
                };
            }
            else
                return null!;
        }
    }
}
