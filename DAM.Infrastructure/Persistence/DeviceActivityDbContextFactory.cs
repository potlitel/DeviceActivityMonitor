using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DAM.Infrastructure.Persistence
{
    public class DeviceActivityDbContextFactory : IDesignTimeDbContextFactory<DeviceActivityDbContext>
    {
        public DeviceActivityDbContext CreateDbContext(string[] args)
        {
            // 1. Localizar el archivo appsettings.json en el proyecto Host
            // Ajustamos la ruta para subir un nivel y entrar en el proyecto del Windows Service
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../DAM.Host.WindowsService"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<DeviceActivityDbContext>();

            // 2. Obtener la cadena de conexión de SQLite
            var connectionString = configuration.GetConnectionString("SQLiteConnection");

            // 3. Configurar el contexto para usar SQLite
            optionsBuilder.UseSqlite(connectionString);

            return new DeviceActivityDbContext(optionsBuilder.Options);
        }
    }
}
