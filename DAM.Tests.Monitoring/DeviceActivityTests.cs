using DAM.Core.Entities;

namespace DAM.Tests.Monitoring
{
    public class DeviceActivityTests
    {
        [Fact]
        public void TimeInserted_CalculatesCorrectly_WhenExtractedAtIsPresent()
        {
            // Arrange
            var insertedAt = new DateTime(2025, 1, 1, 10, 0, 0);
            var extractedAt = new DateTime(2025, 1, 1, 10, 5, 30); // 5 minutos y 30 segundos después

            var activity = new DeviceActivity
            {
                InsertedAt = insertedAt,
                ExtractedAt = extractedAt
            };

            // Act
            var timeInserted = activity.TimeInserted;

            // Assert
            Assert.Equal(new TimeSpan(0, 5, 30), timeInserted);
        }

        [Fact]
        public void TimeInserted_CalculatesUsingNow_WhenExtractedAtIsNull()
        {
            // Arrange
            var insertedAt = DateTime.UtcNow.AddMinutes(-5); // Hace 5 minutos

            var activity = new DeviceActivity
            {
                InsertedAt = insertedAt,
                // ExtractedAt es null
            };

            // Act
            var calculatedDuration = activity.CalculatedDuration;

            // Assert: Aumentamos la tolerancia, buscando un intervalo de 4 a 6 minutos.
            // Esto es mucho más seguro para el entorno de pruebas.
            Assert.True(calculatedDuration.TotalMinutes > 4.0 && calculatedDuration.TotalMinutes < 6.0,
                $"Tiempo calculado: {calculatedDuration.TotalMinutes:F2} min. Esperado entre 4.0 y 6.0 min.");
        }
    }
}
