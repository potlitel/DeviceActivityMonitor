using DAM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAM.Infrastructure.Migrations.Configurations
{
    public class DeviceActivityConfiguration : IEntityTypeConfiguration<DeviceActivity>
    {
        public void Configure(EntityTypeBuilder<DeviceActivity> builder)
        {
            // Configuraciones de DeviceActivity
            builder.Property(e => e.TimeInserted)
                   .UsePropertyAccessMode(PropertyAccessMode.PreferField)
                   .HasColumnName("TimeInserted")
                   .HasColumnType("time(7)")
                   .IsRequired(false);

            builder.Ignore(e => e.CalculatedDuration);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.SerialNumber).IsRequired().HasMaxLength(50);

            // Configuraciones de conversión (HasConversion)
            builder.Property(e => e.FilesCopied).HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
            builder.Property(e => e.FilesDeleted).HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        }
    }
}
