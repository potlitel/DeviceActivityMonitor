using DAM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAM.Infrastructure.Migrations.Configurations
{
    public class DevicePresenceConfiguration : IEntityTypeConfiguration<DevicePresence>
    {
        public void Configure(EntityTypeBuilder<DevicePresence> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.SerialNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Timestamp).IsRequired()
                   .HasColumnType("datetime");
            builder.HasOne(e => e.DeviceActivity) 
                   .WithMany(a => a.PresenceHistory) 
                   .HasForeignKey(e => e.DeviceActivityId) 
                   .IsRequired();
        }
    }
}
