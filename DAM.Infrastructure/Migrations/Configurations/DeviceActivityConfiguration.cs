using DAM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

            builder.Property(e => e.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Ignore(e => e.CalculatedDuration);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.SerialNumber).IsRequired().HasMaxLength(50);

            var stringListComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

            // Configuraciones de conversión (HasConversion)
            builder.Property(e => e.FilesCopied).HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()).Metadata.SetValueComparer(stringListComparer);
            builder.Property(e => e.FilesDeleted).HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()).Metadata.SetValueComparer(stringListComparer);
            builder.HasMany(e => e.PresenceHistory)
                   .WithOne(p => p.DeviceActivity)
                   .HasForeignKey(p => p.DeviceActivityId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Invoices)
                   .WithOne(i => i.DeviceActivity)
                   .HasForeignKey(i => i.DeviceActivityId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
