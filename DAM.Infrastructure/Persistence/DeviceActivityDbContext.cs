using DAM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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
        /// Conjunto de datos (tabla) para el historial de presencia de los dispositivos.
        /// </summary>
        public DbSet<DevicePresence> DevicePresences { get; set; } = null!;

        /// <summary>
        /// Conjunto de datos (tabla) para las facturas de los dispositivos.
        /// </summary>
        public DbSet<Invoice> Invoices { get; set; } = null!;

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
            // Carga la configuración para cada entidad
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
