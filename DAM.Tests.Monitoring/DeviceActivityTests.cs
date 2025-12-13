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

            // Act (La precisión no será perfecta, pero debe estar cerca de 5 minutos)
            var timeInserted = activity.TimeInserted;

            // Assert
            Assert.True(timeInserted.TotalMinutes > 4.9 && timeInserted.TotalMinutes < 5.1);
        }
    }
}
