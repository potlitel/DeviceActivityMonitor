# 🚀 Estrategia de Pruebas: `DAM.Tests.Monitoring`

### 

Este proyecto 🏗️ alberga todas las pruebas automatizadas **XUnit** implementadas para validar el **mecanismo central de monitoreo y persistencia de datos** del servicio `DAM.Host.WindowsService`.

Nuestro objetivo es garantizar la **fiabilidad del código** y la **correcta integración** de todos los componentes clave.

* * *

## 🔍 1. Segmentación y Tipos de Pruebas Implementadas

### 

Hemos diseñado una estrategia de pruebas en capas para cubrir el servicio desde la **lógica pura** hasta el **flujo de negocio completo**.

### 1.1. 🔬 Pruebas Unitarias (Pure Logic)

### 

Estas pruebas se centran en verificar **pequeñas unidades de código de forma aislada**, sin dependencias externas.

# 

| **Clase** | **🎯 Objetivo** | **✨ Enfoque Clave** |
| --- | --- | --- |
| `DeviceActivityTests` | **Entidad de Dominio: `DeviceActivity`** | Valida la lógica de las **propiedades calculadas** (ej. `TimeInserted`), asegurando que el cálculo de la diferencia de tiempo es **correcto y robusto** bajo diversas condiciones. |

### 1.2. 🔗 Pruebas de Integración (Persistencia de Datos)

# 

Aquí validamos que los componentes interactúan correctamente, especialmente la comunicación con la capa de datos.

### 

| **Clase** | **🎯 Objetivo** | **✨ Enfoque Clave** |
| --- | --- | --- |
| `PersistenceTests` | **Capa de Datos (EF Core)** | Utiliza una **Base de Datos en Memoria** (`Microsoft.EntityFrameworkCore.InMemory`) para verificar que la configuración de Entity Framework Core (`DeviceActivityContext`) mapea y persiste correctamente **todas** las propiedades de la entidad `DeviceActivity`. Esto incluye campos complejos como la **serialización de listas** (ej. `FilesCopied`). |

### 1.3. 🔁 Pruebas E2E Simuladas (Flujo de Negocio Completo)

### 

Estas pruebas simulan el comportamiento del sistema como un todo, desde la entrada hasta la salida esperada.

### 

| **Clase** | **🎯 Objetivo** | **✨ Enfoque Clave** |
| --- | --- | --- |
| `WorkerServiceTests` | **Ciclo de Vida del Servicio Principal (`Worker`)** | Simula el flujo completo de: 🔌 Conexión (`IDeviceMonitor.DeviceConnected`) $\rightarrow$ 🏃‍♀️ Monitoreo $\\rightarrow$ 🛑 Desconexión (`IDeviceMonitor.DeviceDisconnected`). |
|  |  | Se emplea el patrón **Factory Mocking** (`IDeviceActivityWatcherFactory`) para **controlar el resultado** del `Watcher` y verificar que el `Worker` llama **exactamente una vez** al servicio de persistencia (`IActivityRepository.AddActivityAsync`) con los datos clave correctos (ej. `SerialNumber`). **¡Garantía del flujo de trabajo!** |

## 🛠️ 2. Ejecución de las Pruebas

### 

Para poner a prueba el proyecto, sigue estas sencillas instrucciones.

### ✅ Requisitos

### 

-   Asegúrese de tener configurado el entorno **.NET CLI** (SDK).
    

### 🚀 Comando de Ejecución Principal

### 

Para iniciar **todas** las pruebas del proyecto `DAM.Tests.Monitoring`, navegue a la raíz de la solución y utilice el siguiente comando:

```bash
dotnet test DAM.Tests.Monitoring/DAM.Tests.Monitoring.csproj
```

## 

**Tip Rápido:** Si la estructura de carpetas coincide con el nombre del proyecto, a menudo puede usar la forma abreviada:

```bash
    dotnet test DAM.Tests.Monitoring
```

### ⚙️ Opciones Avanzadas de Filtrado y Control

## 

Utilice estas banderas para afinar su ejecución, acelerar su ciclo de desarrollo, o depurar resultados específicos:

### 

| **Comando** | **✨ Propósito Detallado** |
| --- | --- |
| `dotnet test --no-build` | ⚡️ Ejecuta las pruebas **sin compilar primero**. ¡Ahorre tiempo! Útil si ya compiló la solución y solo modificó las pruebas. |
| `dotnet test --logger "console;verbosity=normal"` | 📢 Muestra **información de la ejecución más detallada** en la consola, lo que facilita la depuración y seguimiento. |
| `dotnet test --filter FullClassName=DAM.Tests.Monitoring.WorkerServiceTests` | 🎯 **Filtra la ejecución**. Ejecuta **únicamente** las pruebas contenidas en la clase `WorkerServiceTests`. |

## 💖 Contribuciones

### 

Si encuentra un _bug_ 🐛 o desea proponer una prueba, ¡su contribución es bienvenida!