# 🧪 Estrategia de Pruebas: DAM.Tests.Monitoring

Este proyecto contiene todas las pruebas automatizadas implementadas para validar el mecanismo central de monitoreo y persistencia de datos del servicio `DAM.Host.WindowsService`.

Las pruebas están diseñadas para garantizar la fiabilidad del código y la correcta integración de los componentes clave.

---

## 1. Tipos de Pruebas Implementadas

Hemos segmentado las pruebas para cubrir diferentes aspectos del servicio:

### 1.1 Pruebas Unitarias (Pure Logic)

| Clase | Objetivo | Enfoque |
| :--- | :--- | :--- |
| `DeviceActivityTests` | **Entidad `DeviceActivity`** | Valida la lógica de las propiedades calculadas, específicamente `TimeInserted`, asegurando que el cálculo de la diferencia de tiempo es correcto bajo varias condiciones. |

### 1.2 Pruebas de Integración (Persistencia)

| Clase | Objetivo | Enfoque |
| :--- | :--- | :--- |
| `PersistenceTests` | **Capa de Datos (EF Core)** | Utiliza una base de datos en memoria (`Microsoft.EntityFrameworkCore.InMemory`) para verificar que la configuración de Entity Framework Core (`DeviceActivityContext`) mapea correctamente **todas** las propiedades de la entidad `DeviceActivity` (incluyendo `SerialNumber`, `MegabytesCopied`, y la serialización de listas como `FilesCopied`). |

### 1.3 Pruebas E2E Simuladas (Flujo de Negocio)

| Clase | Objetivo | Enfoque |
| :--- | :--- | :--- |
| `WorkerServiceTests` | **Ciclo de Vida del Servicio** | Simula el flujo completo de conexión (`IDeviceMonitor.DeviceConnected`) y desconexión (`IDeviceMonitor.DeviceDisconnected`) de un dispositivo. |
| | | Se utiliza el patrón **Factory Mocking** (`IDeviceActivityWatcherFactory`) para controlar el resultado del `Watcher` y verificar que el `Worker` llama **exactamente una vez** al servicio de persistencia (`IActivityRepository.AddActivityAsync`) con los datos clave correctos (ej. `SerialNumber`). |

---

## 2. Ejecución de las Pruebas

### ⚙️ Requisitos

Asegúrese de que el entorno de .NET CLI esté configurado.

### 🚀 Comando Principal

Para ejecutar todas las pruebas dentro del proyecto `DAM.Tests.Monitoring`, utilice el siguiente comando desde la raíz de la solución:

```bash
dotnet test DAM.Tests.Monitoring/DAM.Tests.Monitoring.csproj
```

### 

O, si el nombre de la carpeta y el proyecto coinciden, a veces es suficiente con solo el nombre de la carpeta:

```bash
dotnet test DAM.Tests.Monitoring
```

### Opciones Adicionales

### 

| **Comando** | **Propósito** |
| --- | --- |
| `dotnet test --no-build` | Ejecuta las pruebas sin compilar. Útil si ya compiló la solución previamente. |
| `dotnet test --logger "console;verbosity=normal"` | Muestra información detallada de la ejecución en la consola. |
| `dotnet test --filter FullClassName=DAM.Tests.Monitoring.WorkerServiceTests` | Ejecuta únicamente las pruebas dentro de la clase `WorkerServiceTests`. |