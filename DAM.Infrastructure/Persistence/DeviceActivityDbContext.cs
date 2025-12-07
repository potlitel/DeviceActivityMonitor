using DAM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAM.Infrastructure.Persistence
{
    public class DeviceActivityDbContext : DbContext
    {
        // Conjuntos de datos (Tablas)
        public DbSet<DeviceActivity> DeviceActivities { get; set; } = null!;
        public DbSet<ServiceEvent> ServiceEvents { get; set; } = null!;

        public DeviceActivityDbContext(DbContextOptions<DeviceActivityDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de DeviceActivity
            modelBuilder.Entity<DeviceActivity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FilesCopied).HasConversion(
                    v => string.Join(';', v), // Convertir List<string> a string para SQLite
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList() // Convertir string a List<string>
                );
                entity.Property(e => e.FilesDeleted).HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
            });

            // Configuración de ServiceEvent
            modelBuilder.Entity<ServiceEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            });
        }
    }
}
