using DAM.Core.Interfaces;
using DAM.Host.WindowsService;
using DAM.Host.WindowsService.Monitoring;
using DAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DAM.Infrastructure.Storage;
using DAM.Infrastructure.Utils;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices((hostContext, services) => // Usa hostContext para acceder a config
    {
        // 1. Configuración de BD (SQLite)
        string dbPath = Path.Combine(AppContext.BaseDirectory, "DeviceActivityMonitor.db");
        services.AddDbContext<DeviceActivityDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // 2. Configuración de HttpClient (para API)
        services.AddHttpClient<IApiStatusChecker, ApiStatusChecker>(client =>
        {
            // La URL base debe venir de la configuración (appsettings.json)
            client.BaseAddress = new Uri(hostContext.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/");
        });
        services.AddHttpClient<ApiStorageService>(client =>
        {
            client.BaseAddress = new Uri(hostContext.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/");
        });

        // 3. Registro de Servicios y Estrategias (IoC)
        services.AddScoped<IActivityRepository, ActivityRepository>();

        // **Registro de los servicios concretos de almacenamiento**
        services.AddScoped<LocalDbStorageService>(); // Registrados directamente
        // ApiStorageService ya está registrado via AddHttpClient

        // **El servicio principal que decide la estrategia**
        services.AddScoped<IActivityStorageService, ResilientStorageService>();

        // 4. Servicios del Host
        services.AddSingleton<IDeviceMonitor, WmiDeviceMonitor>();
        services.AddHostedService<Worker>(); // El servicio principal
    })
    .Build();

// Asegurar que la base de datos se crea al inicio
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DeviceActivityDbContext>();
    dbContext.Database.Migrate();
}

await host.RunAsync();