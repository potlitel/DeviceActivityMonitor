using DAM.Core.Entities;
using DAM.Core.Interfaces;
using DAM.Host.WindowsService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DAM.Tests.Monitoring
{
    public class WorkerServiceTests
    {
        [Fact]
        public async Task DeviceDisconnected_ShouldFinalizeActivityAndPersist()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Worker>>();
            var mockDeviceMonitor = new Mock<IDeviceMonitor>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Simulación de ScopeFactory y Repositorio (IActivityRepository)
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockActivityRepository = new Mock<IActivityRepository>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IActivityRepository)))
                               .Returns(mockActivityRepository.Object);

            var mockScope = new Mock<IServiceScope>();
            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

            var worker = new Worker(
                mockLogger.Object,
                mockDeviceMonitor.Object,
                mockLoggerFactory.Object,
                mockScopeFactory.Object);

            var driveLetter = "G";
            // --- VALOR ESPERADO ---
            // Asumimos que el Watcher logró obtener el número de serie de la unidad.
            var expectedSerialNumber = "USB-DEV-7890";

            // Simular el evento de conexión.
            // NOTA CRÍTICA: En un test ideal, aquí mockearíamos al Watcher para forzar
            // que el objeto DeviceActivity tenga este 'expectedSerialNumber'.
            mockDeviceMonitor.Raise(n => n.DeviceConnected += null, driveLetter);

            // Act: Simular el evento de desconexión
            mockDeviceMonitor.Raise(n => n.DeviceDisconnected += null, driveLetter);

            await Task.Delay(100);

            // Assert

            // 2. Verificar que el repositorio fue llamado para guardar la actividad, 
            // y que el objeto guardado tiene el SerialNumber que esperábamos.
            mockActivityRepository.Verify(
                r => r.AddActivityAsync(
                    // AFIRMACIÓN SOBRE LA PROPIEDAD: Verificamos el SerialNumber
                    It.Is<DeviceActivity>(a => a.SerialNumber == expectedSerialNumber)),
                Times.Once,
                "El método AddActivityAsync del repositorio NO fue llamado con el SerialNumber esperado."
            );
        }
    }
}
