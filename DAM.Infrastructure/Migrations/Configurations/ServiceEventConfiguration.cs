using DAM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAM.Infrastructure.Migrations.Configurations
{
    public class ServiceEventConfiguration : IEntityTypeConfiguration<ServiceEvent>
    {
        public void Configure(EntityTypeBuilder<ServiceEvent> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Timestamp).IsRequired();
            builder.Property(e => e.EventType).IsRequired().HasMaxLength(50);
        }
    }
}
