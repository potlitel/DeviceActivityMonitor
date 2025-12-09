using DAM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAM.Infrastructure.Persistence
{
    /// <summary>
    /// Contexto de la base de datos para Entity Framework Core que maneja la persistencia de la actividad de dispositivos.
    /// </summary>
    public class DeviceActivityDbContext : DbContext
    {
        /// <summary>
        /// Conjunto de datos (tabla) para las actividades de los dispositivos.
        /// </summary>
        public DbSet<DeviceActivity> DeviceActivities { get; set; } = null!;

        /// <summary>
        /// Conjunto de datos (tabla) para los eventos internos del servicio.
        /// </summary>
        public DbSet<ServiceEvent> ServiceEvents { get; set; } = null!;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="DeviceActivityDbContext"/>.
        /// </summary>
        /// <param name="options">Opciones de configuración del contexto (usualmente para la cadena de conexión).</param>
        public DeviceActivityDbContext(DbContextOptions<DeviceActivityDbContext> options)
            : base(options)
        {
        }

        /// <inheritdoc/>
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
