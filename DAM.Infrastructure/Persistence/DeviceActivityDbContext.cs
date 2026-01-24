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
        /// Configura SQLite en modo WAL (Write-Ahead Logging) para alto rendimiento y concurrencia.
        /// </summary>
        /// <param name="options">Opciones de configuración del contexto.</param>
        public DeviceActivityDbContext(DbContextOptions<DeviceActivityDbContext> options)
            : base(options)
        {
            // Configuración de SQLite para escenarios de alta carga (I/O intensivo)
            var connection = Database.GetDbConnection();

            // Es importante abrir la conexión para ejecutar los comandos PRAGMA
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            using var command = connection.CreateCommand();
            // 1. Activa Write-Ahead Logging para permitir lecturas y escrituras simultáneas
            command.CommandText = "PRAGMA journal_mode=WAL;";
            command.ExecuteNonQuery();

            // 2. Modo Normal: Equilibrio perfecto entre velocidad y seguridad en modo WAL
            command.CommandText = "PRAGMA synchronous=NORMAL;";
            command.ExecuteNonQuery();

            // 3. Tiempo de espera de 5 segundos si la base de datos está ocupada antes de lanzar error
            command.CommandText = "PRAGMA busy_timeout=5000;";
            command.ExecuteNonQuery();
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Carga la configuración para cada entidad
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
