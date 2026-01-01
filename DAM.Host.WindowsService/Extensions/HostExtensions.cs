using DAM.Core.Constants;
using DAM.Core.Interfaces;
using DAM.Core.Settings;
using DAM.Host.WindowsService.Monitoring;
using DAM.Host.WindowsService.Monitoring.Interfaces;
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
            //string dbPath = Path.Combine(AppContext.BaseDirectory, configuration["Database:Name"] ?? "DeviceActivityMonitor.db");
            string dbName = configuration["Database:Name"] ?? DataConstants.DefaultDbName;
            string dbPath = Path.Combine(AppContext.BaseDirectory, dbName);

            services.AddDbContext<DeviceActivityDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            services.AddScoped<IActivityRepository, ActivityRepository>();
            // Es vital que sea Scoped para que maneje la transacción del DbContext actual
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IDevicePersistenceService, DevicePersistenceService>();
            services.AddSingleton<IInvoiceCalculator, FixedPriceInvoiceCalculator>();
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
            //var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/";
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? DataConstants.DefaultApiUrl;

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

            // IMPORTANTE: ResilientStorageService(El servicio resiliente que alterna entre BD local y API remota.) debe ser Scoped porque
            // depende de LocalDbStorageService (que usa el UoW/DbContext)
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
            // Registramos la Factoría, ya que el Worker la usa para CREAR el Watcher
            services.AddSingleton<IDeviceActivityWatcherFactory, DeviceActivityWatcherFactory>();
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
                    logger.LogError(ex, Messages.Repository.MigrationError);
                    throw;
                }
            }
            return host;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns>La colección de servicios para encadenamiento.</returns>
        public static IServiceCollection AddApplicationSettings(this IServiceCollection services, IConfiguration configuration) {

            services.Configure<InvoiceSettings>(configuration.GetSection(DataConstants.ConfigSections.InvoiceSettings));
            services.Configure<StorageSettings>(configuration.GetSection(DataConstants.ConfigSections.StorageSettings));

            return services;

        }
    }
}
