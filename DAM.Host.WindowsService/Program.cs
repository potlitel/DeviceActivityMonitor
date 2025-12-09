using DAM.Host.WindowsService.Extensions;

IHost host = Host.CreateDefaultBuilder(args)
    // 1. Configuración específica para ser un servicio de Windows
    .UseWindowsService()

    // 2. Configuración de Servicios usando los métodos de extensión
    .ConfigureServices((hostContext, services) =>
    {
        // Persistencia (SQLite y Repositorios)
        services.AddSqlitePersistence(hostContext.Configuration);

        // Integración (API y Resiliencia)
        services.AddWebApiIntegration(hostContext.Configuration);

        // Lógica de Monitoreo y Host (Worker)
        services.AddDeviceMonitoringHost();
    })
    .Build();

// 3. Aplicar migraciones antes de iniciar el host
host.MigrateDatabase();

await host.RunAsync();