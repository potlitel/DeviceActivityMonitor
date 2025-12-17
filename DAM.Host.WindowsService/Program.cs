using DAM.Host.WindowsService.Extensions;

// Definimos la clase Program que contiene el punto de entrada Main
public class Program
{
    public static async Task Main(string[] args)
    {
        // 💡 1. AJUSTE CRÍTICO: Forzar el directorio de trabajo
        // Esto garantiza que el host encuentre appsettings.json, la base de datos SQLite 
        // y otros archivos en el directorio donde reside el servicio (BaseDirectory),
        // y no en C:\Windows\System32.
        System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

        // El resto de la construcción del host se mantiene igual
        IHost host = Host.CreateDefaultBuilder(args)
            .UseContentRoot(System.AppDomain.CurrentDomain.BaseDirectory)
            // Usa el método ConfigureWindowsService() si es el que usas para envolver UseWindowsService(), 
            // si no, usa el método directo del framework:
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

                services.AddApplicationSettings(hostContext.Configuration);
            })
            .Build();

        // 3. Aplicar migraciones antes de iniciar el host
        try
        {
            host.MigrateDatabase();
        }
        catch (Exception ex)
        {
            // Opcional: Obtener el logger y registrar el error
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "FATAL: La migración de la base de datos (SQLite) falló. El servicio no puede iniciar.");

            // Re-lanzar o terminar elegantemente para evitar que RunAsync falle ciegamente.
            // En un servicio de Windows, lo mejor es terminar con un código de error.
            return;
        }

        await host.RunAsync();
    }
}