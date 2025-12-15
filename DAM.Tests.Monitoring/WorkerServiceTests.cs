using DAM.Core.Entities;
using DAM.Core.Interfaces;
using DAM.Host.WindowsService;
using DAM.Host.WindowsService.Monitoring;
using DAM.Host.WindowsService.Monitoring.Interfaces;
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
            var driveLetter = "G";
            var expectedSerialNumber = "USB-DEV-7890";

            // OBJETO DE SINCRONIZACIÓN ASÍNCRONA
            var persistenceCompleted = new ManualResetEventSlim(false);

            // Arrange (Inicialización de Mocks)
            var mockLogger = new Mock<ILogger<Worker>>();
            var mockDeviceMonitor = new Mock<IDeviceMonitor>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            var mockWatcherFactory = new Mock<IDeviceActivityWatcherFactory>();
            var mockWatcher = new Mock<IDeviceActivityWatcher>();

            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockActivityRepository = new Mock<IActivityRepository>();
            var mockStorageService = new Mock<IActivityStorageService>(); // El servicio Scoped

            var expectedActivity = new DeviceActivity
            {
                SerialNumber = expectedSerialNumber
            };

            // CONFIGURACIÓN DEL WATCHER MOCKEADO
            mockWatcher.SetupGet(w => w.CurrentActivity).Returns(expectedActivity);

            mockWatcher.Setup(w => w.FinalizeActivity())
                       .Callback(() => mockWatcher.Raise(w => w.ActivityCompleted += null, expectedActivity));

            mockWatcherFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<ILogger<DeviceActivityWatcher>>()))
                              .Returns(mockWatcher.Object);

            // --- CONFIGURACIÓN DE LA CADENA DE PERSISTENCIA (CRÍTICO) ---

            // 1. Configurar el Repositorio (Punto Final de la cadena y Liberador de la Prueba)
            mockActivityRepository.Setup(r => r.AddActivityAsync(It.IsAny<DeviceActivity>()))
                .Returns(Task.CompletedTask)
                .Callback(() =>
                {
                    // Cuando la invocación del repositorio ocurre, liberamos la espera.
                    persistenceCompleted.Set();
                });

            // 2. Configurar el StorageService (Llamado por el Worker)
            mockStorageService.Setup(s => s.StoreActivityAsync(
                It.Is<DeviceActivity>(a => a.SerialNumber == expectedSerialNumber)))
                .Returns(Task.CompletedTask)
                .Callback((DeviceActivity a) =>
                {
                    // Llama al Repositorio de forma SÍNCRONA para que se ejecute el Callback.Set() inmediatamente.
                    // Esto asegura que la invocación se registre y la señal se envíe.
                    mockActivityRepository.Object.AddActivityAsync(a);
                });

            // 3. Configurar el ServiceProvider para que devuelva el Mock del StorageService.
            // Usamos GetService(typeof(T)) que es el método base de GetRequiredService<T>.
            // ¡Aquí está la clave! Tienes que asegurarte de que el Type sea exactamente la interfaz.
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IActivityStorageService)))
                               .Returns(mockStorageService.Object);

            // 4. Configurar Scope
            var mockScope = new Mock<IServiceScope>();
            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

            // Instanciar el Worker
            var worker = new Worker(
                mockLogger.Object,
                mockDeviceMonitor.Object,
                mockLoggerFactory.Object,
                mockScopeFactory.Object,
                mockWatcherFactory.Object);

            // Act
            // 1. Simular conexión (añade el watcher y suscribe el handler)
            mockDeviceMonitor.Raise(n => n.DeviceConnected += null, driveLetter);

            // 2. Simular desconexión (llama a FinalizeActivity, que dispara el handler async void)
            mockDeviceMonitor.Raise(n => n.DeviceDisconnected += null, driveLetter);

            // ** 3. FORZAR ESPERA DE LA TAREA ASÍNCRONA (Usando Task.Run para evitar bloqueo de hilo principal) **
            var persistenceTask = Task.Run(() => persistenceCompleted.Wait(TimeSpan.FromSeconds(10)));
            var success = await persistenceTask;

            // Assert
            Assert.True(success, "La operación de persistencia asíncrona no se completó en el tiempo límite (10 segundos).");

            mockActivityRepository.Verify(
                r => r.AddActivityAsync(
                    It.Is<DeviceActivity>(a => a.SerialNumber == expectedSerialNumber)),
                Times.Once,
                "Fallo en el flujo E2E: El SerialNumber o DriveLetter no se pasaron correctamente a la persistencia."
            );
        }
    }
}
