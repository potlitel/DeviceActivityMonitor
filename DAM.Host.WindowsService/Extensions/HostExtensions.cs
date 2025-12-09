using DAM.Core.Interfaces;
using DAM.Host.WindowsService.Monitoring;
using DAM.Infrastructure.Persistence;
using DAM.Infrastructure.Storage;
using DAM.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace DAM.Host.WindowsService.Extensions
{
    /// <summary>
    /// Métodos de extensión para configurar y mejorar la legibilidad del IHost de la aplicación Worker.
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Configura y registra el contexto de base de datos SQLite y realiza la migración inicial.
        /// </summary>
        /// <param name="services">Colección de servicios de la aplicación.</param>
        /// <param name="configuration">Configuración de la aplicación.</param>
        /// <returns>La colección de servicios para encadenamiento.</returns>
        public static IServiceCollection AddSqlitePersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // El path de la base de datos se resuelve en el directorio de la aplicación para que sea portátil.
            string dbPath = Path.Combine(AppContext.BaseDirectory, configuration["Database:Name"] ?? "DeviceActivityMonitor.db");

            services.AddDbContext<DeviceActivityDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Se registra la capa de repositorio y la lógica de almacenamiento local.
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<LocalDbStorageService>();

            return services;
        }

        /// <summary>
        /// Configura los servicios de HttpClient para la comunicación con la Web API y la verificación de estado (Health Check).
        /// </summary>
        /// <param name="services">Colección de servicios de la aplicación.</param>
        /// <param name="configuration">Configuración de la aplicación.</param>
        /// <returns>La colección de servicios para encadenamiento.</returns>
        public static IServiceCollection AddWebApiIntegration(this IServiceCollection services, IConfiguration configuration)
        {
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/";

            // HttpClient para el Checker de Disponibilidad de API
            services.AddHttpClient<IApiStatusChecker, ApiStatusChecker>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(5); // Tiempo de espera corto para evitar bloqueos largos
            });

            // HttpClient para el Servicio de Almacenamiento Remoto
            services.AddHttpClient<ApiStorageService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            });

            // El servicio resiliente que alterna entre BD local y API remota.
            services.AddScoped<IActivityStorageService, ResilientStorageService>();

            return services;
        }

        /// <summary>
        /// Configura los servicios principales de monitoreo y el Worker IHostedService.
        /// </summary>
        /// <param name="services">Colección de servicios de la aplicación.</param>
        /// <returns>La colección de servicios para encadenamiento.</returns>
        public static IServiceCollection AddDeviceMonitoringHost(this IServiceCollection services)
        {
            // El monitor de eventos de hardware (WMI)
            services.AddSingleton<IDeviceMonitor, WmiDeviceMonitor>();

            // El servicio principal de Windows que hereda de BackgroundService
            services.AddHostedService<Worker>();

            return services;
        }

        /// <summary>
        /// Asegura que la base de datos se crea y que las migraciones pendientes se apliquen al inicio.
        /// </summary>
        /// <param name="host">El IHost de la aplicación.</param>
        /// <returns>El IHost para encadenamiento.</returns>
        public static IHost MigrateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dbContext = services.GetRequiredService<DeviceActivityDbContext>();
                    dbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    // Manejo de errores: Si la migración falla, el servicio debe ser capaz de reportarlo.
                    // En un ambiente real, se usaría un ILogger para reportar esto.
                    var logger = services.GetRequiredService<ILogger<Worker>>();
                    logger.LogError(ex, "Ocurrió un error durante la migración de la base de datos.");
                    throw;
                }
            }
            return host;
        }
    }
}
