
# ⚙️ Historial de Migraciones de la Base de Datos (Entity Framework Core)

Este documento registra cronológicamente todas las migraciones aplicadas a la base de datos utilizando Entity Framework Core, especificando los comandos utilizados.

## 🚀 Proyectos Involucrados


| **Rol** | **Proyecto** | **Descripción** |
| --- | --- | --- |
| **Proyecto de Infraestructura** | `DAM.Infrastructure/DAM.Infrastructure.csproj` | Contiene el `DbContext` y las clases de migración. |
| **Proyecto de Inicio (Startup)** | `DAM.Host.WindowsService/DAM.Host.WindowsService.csproj` | Proyecto que se utiliza para ejecutar los comandos de EF Core. |

## 🛠️ Comandos de Migración Registrados

La siguiente tabla detalla las migraciones generadas y el comando exacto de `.NET EF` utilizado para crearlas.

## 

| **Fecha de Creación (Estimada)** | **Nombre de la Migración** | **Comando de Creación** | **Descripción Breve** |
| --- | --- | --- | --- |
| \[07/12/2025\] | **InitialCreate** | `dotnet ef migrations add InitialCreate --project DAM.Infrastructure/DAM.Infrastructure.csproj --startup-project DAM.Host.WindowsService/DAM.Host.WindowsService.csproj` | Creación inicial del esquema de la base de datos. |
| \[15/12/2025\] | **AddTimeInsertedToPersistOnDB** | `dotnet ef migrations add AddTimeInsertedToPersistOnDB --project DAM.Infrastructure/DAM.Infrastructure.csproj --startup-project DAM.Host.WindowsService/DAM.Host.WindowsService.csproj` | Añade una columna para registrar el tiempo de inserción en una tabla específica. |
| \[15/12/2025\] | **AddDevicePresenceEntity** | `dotnet ef migrations add AddDevicePresenceEntity --project DAM.Infrastructure/DAM.Infrastructure.csproj --startup-project DAM.Host.WindowsService/DAM.Host.WindowsService.csproj` | Añade entidad para registrar el historial de presencia de dispositivos. |
| \[15/12/2025\] | **AddInvoiceEntity** | `dotnet ef migrations add AddInvoiceEntity --project DAM.Infrastructure/DAM.Infrastructure.csproj --startup-project DAM.Host.WindowsService/DAM.Host.WindowsService.csproj` | Añade entidad para registrar la factura sobre cada operación de copiado en cada dispositivo. |

## 🔄 Comando de Aplicación (Update)

Una vez que la migración ha sido creada y documentada, aplícala a tu base de datos (local o de desarrollo) usando el comando de update:

```bash
dotnet ef database update --project DAM.Infrastructure/DAM.Infrastructure.csproj
```

## 📝 Plantilla para Generar la Siguiente Migración

## 

Para generar una nueva migración, simplemente utiliza el siguiente formato, asegurándote de reemplazar el marcador de posición por un nombre descriptivo:

```bash
    dotnet ef migrations add [NombreDescriptivoDeTuMigracion] \
    --project DAM.Infrastructure/DAM.Infrastructure.csproj \
    --startup-project DAM.Host.WindowsService/DAM.Host.WindowsService.csproj
```    

> **Ejemplo:** Si estás añadiendo la funcionalidad de registro de logs, el comando podría ser:
> 
> `dotnet ef migrations add AddLogTableAndIndexes --project DAM.Infrastructure/DAM.Infrastructure.csproj --startup-project DAM.Host.WindowsService/DAM.Host.WindowsService.csproj`