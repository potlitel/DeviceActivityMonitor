using DAM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Infrastructure.Migrations.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        /// <summary>
        /// Configura el esquema de la base de datos para la entidad Invoice.
        /// </summary>
        /// <param name="builder">Constructor de tipo de entidad proporcionado por Entity Framework Core.</param>
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.SerialNumber).IsRequired().HasMaxLength(50);

            // La marca de tiempo de la factura es un campo obligatorio para registrar la fecha de emisión.
            builder.Property(e => e.Timestamp).IsRequired();

            // **Configuración Crítica de Precisión:**
            // TotalAmount se configura explícitamente como "decimal(18, 2)" en la base de datos.
            // Esto es esencial para evitar errores de redondeo y pérdida de precisión
            // al manejar valores monetarios (dinero) en bases de datos relacionales.
            builder.Property(e => e.TotalAmount)
                   .IsRequired()
                   .HasColumnType("decimal(18, 2)");

            // Define una longitud máxima para la descripción, permitiendo que sea opcional (nullable por defecto).
            builder.Property(e => e.Description).HasMaxLength(255);
        }
    }
}
