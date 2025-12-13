using DAM.Core.Entities;
using DAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DAM.Tests.Monitoring
{
    public class PersistenceTests : IDisposable
    {
        private readonly DeviceActivityDbContext _context;

        public PersistenceTests()
        {
            // Configurar un contexto en memoria para cada prueba
            var options = new DbContextOptionsBuilder<DeviceActivityDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Usa un nombre único
                .Options;

            _context = new DeviceActivityDbContext(options);
            _context.Database.EnsureCreated(); // Crea el esquema en memoria
        }

        [Fact]
        public async Task AddActivity_ShouldPersistDataCorrectly()
        {
            // Arrange
            var inserted = DateTime.UtcNow.AddMinutes(-10);
            var extracted = DateTime.UtcNow;

            // Arrange
            var activity = new DeviceActivity
            {
                //DriveLetter = "Z",
                SerialNumber = "ABC-123", // << Agregado para prueba
                Model = "FlashDrive-X",    // << Agregado para prueba
                TotalCapacityMB = 8000,    // << Agregado para prueba
                InsertedAt = inserted,
                ExtractedAt = extracted,
                MegabytesCopied = 512,
                MegabytesDeleted = 100,
                //TimeInserted = extracted - inserted, // Asignamos el valor calculado
                FilesCopied = new List<string> { "a.txt" }, // << Agregado para prueba
                FilesDeleted = new List<string> { "b.txt" }, // << Agregado para prueba
            };

            // Act
            _context.DeviceActivities.Add(activity);
            await _context.SaveChangesAsync();
            _context.Entry(activity).State = EntityState.Detached; // Simular que se lee de nuevo

            // Assert
            var savedActivity = await _context.DeviceActivities.FirstOrDefaultAsync(a => a.MegabytesCopied == 512);

            Assert.NotNull(savedActivity);

            // Validaciones Adicionales (Mapeo de Propiedades Simples)
            Assert.Equal("ABC-123", savedActivity.SerialNumber);
            Assert.Equal("FlashDrive-X", savedActivity.Model);
            Assert.Equal(8000, savedActivity.TotalCapacityMB);

            // Validaciones de Propiedades con Lógica/Cálculo
            Assert.Equal(512, savedActivity.MegabytesCopied);
            Assert.Equal(100, savedActivity.MegabytesDeleted);
            Assert.Equal(activity.TimeInserted, savedActivity.TimeInserted);

            // Validaciones de Listas (Serialización JSON/Value Converter)
            Assert.Single(savedActivity.FilesCopied);
            Assert.Equal("a.txt", savedActivity.FilesCopied[0]);
        }

        // Método de limpieza
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
