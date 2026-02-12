using DAM.Core.Entities;
using DAM.Core.Enums;
using DAM.Infrastructure.Audit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace DAM.Infrastructure.Persistence;

public static class DbInitializer
{
    //public static void Seed(DeviceActivityDbContext context)
    //{
    //    context.Database.EnsureCreated();

    //    if (context.Users.Any()) return; // Si ya hay datos, no hacer nada

    //    // 1. USUARIOS (Password: P@ssw0rd2026)
    //    var manager = new ApplicationUser
    //    {
    //        Id = Guid.NewGuid(),
    //        Username = "admin_manager",
    //        Email = "manager@dam.com",
    //        PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd2026"),
    //        Role = UserRole.Manager,
    //        IsTwoFactorEnabled = true, // Simulamos que ya lo activó
    //        TwoFactorSecret = "JBSWY3DPEHPK3PXP" // Secret de ejemplo
    //    };

    //    var worker = new ApplicationUser
    //    {
    //        Id = Guid.NewGuid(),
    //        Username = "worker_user",
    //        Email = "worker@dam.com",
    //        PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd2026"),
    //        Role = UserRole.Worker,
    //        IsTwoFactorEnabled = false
    //    };

    //    context.Users.AddRange(manager, worker);

    //    // 2. ACTIVIDADES DE DISPOSITIVOS (Historial de los últimos 7 días)
    //    //var activities = new List<DeviceActivity>();
    //    //for (int i = 1; i <= 20; i++)
    //    //{
    //    //    activities.Add(new DeviceActivity
    //    //    {
    //    //        SerialNumber = $"SN-2026-X{i:00}",
    //    //        Status = i % 5 == 0 ? ActivityStatus.Completed : ActivityStatus.Pending,
    //    //        MegabytesCopied = 1024 * i,
    //    //        MegabytesDeleted = 512 * (i / 2),
    //    //        InsertedAt = DateTime.UtcNow.AddDays(-i).AddHours(i)
    //    //    });
    //    //}
    //    //context.DeviceActivities.AddRange(activities);

    //    // 3. EVENTOS DE SISTEMA (La Caja Negra)
    //    //context.ServiceEvents.AddRange(
    //    //    new ServiceEvent { Level = EventLevel.Information, Source = "Watcher", Message = "Sistema de monitoreo iniciado correctamente.", TimestampUtc = DateTime.UtcNow.AddDays(-10) },
    //    //    new ServiceEvent { Level = EventLevel.Warning, Source = "Watcher", Message = "Latencia detectada en disco USB 3.0", TimestampUtc = DateTime.UtcNow.AddDays(-5) },
    //    //    new ServiceEvent { Level = EventLevel.Error, Source = "Persistence", Message = "Error de escritura temporal en SQLite (Database Locked)", TimestampUtc = DateTime.UtcNow.AddDays(-1) }
    //    //);

    //    //// 4. FACTURAS (Para las actividades completadas)
    //    //foreach (var act in activities.Where(a => a.Status == ActivityStatus.Completed))
    //    //{
    //    //    context.Invoices.Add(new Invoice
    //    //    {
    //    //        Id = Guid.NewGuid(),
    //    //        DeviceActivityId = act.Id,
    //    //        InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{act.SerialNumber.Split('-').Last()}",
    //    //        TotalAmount = (decimal)(act.MegabytesCopied * 0.05), // $0.05 por MB
    //    //        GeneratedAt = act.InsertedAt.AddHours(2)
    //    //    });
    //    //}

    //    // 2️⃣ DISPOSITIVOS DE PRUEBA
    //    if (!await context.DeviceActivities.AnyAsync())
    //    {
    //        logger.LogInformation("💾 Creando actividades de dispositivos de prueba...");

    //        var random = new Random();
    //        var activities = new List<DeviceActivity>();
    //        var serialNumbers = new[] { "SN-001-USB", "SN-002-USB", "SN-003-USB", "SN-004-USB", "SN-005-USB" };
    //        var models = new[] { "Kingston DataTraveler 100", "SanDisk Ultra Fit", "Samsung BAR Plus", "Lexar JumpDrive", "PNY Turbo Attache" };

    //        for (int i = 0; i < 50; i++)
    //        {
    //            var insertedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)).AddHours(-random.Next(1, 24));
    //            var extractedAt = random.Next(0, 2) == 0 ? null : insertedAt.AddHours(random.Next(1, 72));
    //            var totalCapacity = random.Next(8, 256); // GB

    //            activities.Add(new DeviceActivity
    //            {
    //                Id = i + 1,
    //                SerialNumber = serialNumbers[random.Next(serialNumbers.Length)],
    //                Model = models[random.Next(models.Length)],
    //                TotalCapacityMB = totalCapacity * 1024,
    //                InsertedAt = insertedAt,
    //                ExtractedAt = extractedAt,
    //                InitialAvailableMB = (totalCapacity * 1024) - random.Next(0, 20) * 1024,
    //                FinalAvailableMB = extractedAt.HasValue ? random.Next(1, totalCapacity) * 1024 : 0
    //            });
    //        }

    //        await context.DeviceActivities.AddRangeAsync(activities);
    //        await context.SaveChangesAsync();

    //        logger.LogInformation("✅ Actividades creadas: {Count}", activities.Count);
    //    }

    //    // 3️⃣ EVENTOS DE PRESENCIA
    //    if (!await context.DevicePresences.AnyAsync())
    //    {
    //        logger.LogInformation("📡 Creando eventos de presencia de prueba...");

    //        var activities = await context.DeviceActivities.ToListAsync();
    //        var random = new Random();
    //        var presences = new List<DevicePresence>();

    //        foreach (var activity in activities)
    //        {
    //            // Cada actividad genera entre 5-20 eventos de presencia
    //            for (int i = 0; i < random.Next(5, 20); i++)
    //            {
    //                presences.Add(new DevicePresence
    //                {
    //                    Id = presences.Count + 1,
    //                    SerialNumber = activity.SerialNumber,
    //                    Timestamp = activity.InsertedAt.Value.AddMinutes(random.Next(1, 60)),
    //                    ActivityId = activity.Id
    //                });
    //            }
    //        }

    //        await context.DevicePresences.AddRangeAsync(presences);
    //        await context.SaveChangesAsync();

    //        logger.LogInformation("✅ Eventos de presencia creados: {Count}", presences.Count);
    //    }

    //    // 5. TRAZAS DE AUDITORÍA (Simulando actividad previa)
    //    context.AuditLogs.AddRange(
    //        new AuditLog { Username = "admin_manager", Action = "LOGIN_SUCCESS", Resource = "/api/identity/login", HttpMethod = "POST", IpAddress = "127.0.0.1", TimestampUtc = DateTime.UtcNow.AddHours(-5) },
    //        new AuditLog { Username = "admin_manager", Action = "GENERATE_INVOICE", Resource = "/api/invoices/calculate", HttpMethod = "POST", IpAddress = "127.0.0.1", TimestampUtc = DateTime.UtcNow.AddHours(-2) }
    //    );

    //    context.SaveChanges();
    //}


    /// <summary>
    /// 🌱 Ejecuta la inicialización completa de la base de datos.
    /// </summary>
    /// <param name="context">Contexto de base de datos</param>
    /// <param name="logger">Logger para seguimiento</param>
    /// <param name="seedTestData">Indica si se deben generar datos de prueba</param>
    /// <returns>Task completado cuando la inicialización finaliza</returns>
    public static async Task SeedAsync(DeviceActivityDbContext context,
                                       ILogger logger,
                                       bool seedTestData = true)
    {
        logger.LogInformation("🚀 Iniciando inicialización de base de datos...");

        // 1️⃣ DATOS MAESTROS - SIEMPRE SE EJECUTAN
        //await SeedMasterDataAsync(context, logger);

        // 2️⃣ DATOS DE PRUEBA - SOLO EN DESARROLLO
        if (seedTestData && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            await SeedTestDataAsync(context, logger);
        }

        logger.LogInformation("✅ Base de datos inicializada correctamente");
    }

    /// <summary>
    /// 🧪 Genera datos de prueba realistas para entornos de desarrollo.
    /// </summary>
    private static async Task SeedTestDataAsync(DeviceActivityDbContext context, ILogger logger)
    {
        logger.LogInformation("🧪 Generando datos de prueba...");

        // 1️⃣ USUARIOS DE PRUEBA
        if (!await context.Users.AnyAsync())
        {
            logger.LogInformation("👤 Creando usuarios de prueba...");

            //var managerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
            //var workerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Worker");

            var users = new[]
            {
                new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    Username = "admin@dam.com",
                    Email = "admin@dam.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", 12),
                    Role = UserRole.Manager
                },
                new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    Username = "manager@dam.com",
                    Email = "manager@dam.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!", 12),
                    Role = UserRole.Manager
                },
                new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    Username = "worker@dam.com",
                    Email = "worker@dam.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Worker123!", 12),
                    Role = UserRole.Manager
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Usuarios creados: {Count}", users.Length);
        }

        // 2️⃣ DISPOSITIVOS DE PRUEBA
        if (!await context.DeviceActivities.AnyAsync())
        {
            logger.LogInformation("💾 Creando actividades de dispositivos de prueba...");

            var random = new Random();
            var activities = new List<DeviceActivity>();
            var serialNumbers = new[] { "SN-001-USB", "SN-002-USB", "SN-003-USB", "SN-004-USB", "SN-005-USB" };
            var models = new[] { "Kingston DataTraveler 100", "SanDisk Ultra Fit", "Samsung BAR Plus", "Lexar JumpDrive", "PNY Turbo Attache" };

            for (int i = 0; i < 50; i++)
            {
                var insertedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)).AddHours(-random.Next(1, 24));
                //var extractedAt = random.Next(0, 2) == 0 ? null : insertedAt.AddHours(random.Next(1, 72));
                var extractedAt = insertedAt.AddHours(random.Next(1, 72));
                var totalCapacity = random.Next(8, 256); // GB

                activities.Add(new DeviceActivity
                {
                    Id = i + 1,
                    SerialNumber = serialNumbers[random.Next(serialNumbers.Length)],
                    Model = models[random.Next(models.Length)],
                    TotalCapacityMB = totalCapacity * 1024,
                    InsertedAt = insertedAt,
                    ExtractedAt = extractedAt,
                    InitialAvailableMB = (totalCapacity * 1024) - random.Next(0, 20) * 1024,
                    //FinalAvailableMB = extractedAt.HasValue ? random.Next(1, totalCapacity) * 1024 : 0
                    FinalAvailableMB = random.Next(1, totalCapacity) * 1024
                });
            }

            await context.DeviceActivities.AddRangeAsync(activities);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Actividades creadas: {Count}", activities.Count);
        }

        // 3️⃣ EVENTOS DE PRESENCIA
        if (!await context.DevicePresences.AnyAsync())
        {
            logger.LogInformation("📡 Creando eventos de presencia de prueba...");

            var activities = await context.DeviceActivities.ToListAsync();
            var random = new Random();
            var presences = new List<DevicePresence>();

            foreach (var activity in activities)
            {
                // Cada actividad genera entre 5-20 eventos de presencia
                for (int i = 0; i < random.Next(5, 20); i++)
                {
                    presences.Add(new DevicePresence
                    {
                        Id = presences.Count + 1,
                        SerialNumber = activity.SerialNumber,
                        Timestamp = activity.InsertedAt.AddMinutes(random.Next(1, 60)),
                        DeviceActivityId = activity.Id
                    });
                }
            }

            await context.DevicePresences.AddRangeAsync(presences);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Eventos de presencia creados: {Count}", presences.Count);
        }

        // 4️⃣ EVENTOS DE SERVICIO (LOGS)
        if (!await context.ServiceEvents.AnyAsync())
        {
            logger.LogInformation("📋 Creando eventos de servicio de prueba...");

            var random = new Random();
            var events = new List<ServiceEvent>();
            var sources = new[] { "DeviceWatcher", "WorkerService", "HealthMonitor", "AuthenticationService" };
            var levels = new[] { EventLevel.Informational, EventLevel.Warning, EventLevel.Error };
            var messages = new[]
            {
                "Dispositivo detectado en puerto USB",
                "Dispositivo removido correctamente",
                "Espacio en disco bajo: 1.2GB disponibles",
                "Servicio iniciado correctamente",
                "Base de datos sincronizada",
                "Error al leer metadatos del dispositivo",
                "Timeout en operación de lectura"
            };

            for (int i = 0; i < 100; i++)
            {
                events.Add(new ServiceEvent
                {
                    Id = i + 1,
                    EventType = levels[random.Next(levels.Length)].ToString(),
                    Message = messages[random.Next(messages.Length)],
                    Timestamp = DateTime.UtcNow.AddHours(-random.Next(1, 168)),
                });
            }

            await context.ServiceEvents.AddRangeAsync(events);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Eventos de servicio creados: {Count}", events.Count);
        }

        // 5️⃣ FACTURAS DE PRUEBA
        if (!await context.Invoices.AnyAsync())
        {
            logger.LogInformation("💰 Creando facturas de prueba...");

            var completedActivities = await context.DeviceActivities
                .Where(a => a.ExtractedAt.HasValue)
                .Take(20)
                .ToListAsync();

            var random = new Random();
            var invoices = new List<Invoice>();

            foreach (var activity in completedActivities)
            {
                var gbProcessed = (activity.InitialAvailableMB - activity.FinalAvailableMB) / 1024.0;
                var baseRate = 5.00m;
                var total = baseRate + (decimal)(gbProcessed * 0.25);

                invoices.Add(new Invoice
                {
                    Id = invoices.Count + 1,
                    SerialNumber = activity.SerialNumber,
                    Timestamp = activity.ExtractedAt!.Value,
                    TotalAmount = Math.Round(total, 2),
                    Description = $"Factura por actividad {activity.Id}: {gbProcessed:F2} GB procesados",
                    DeviceActivityId = activity.Id,
                });
            }

            await context.Invoices.AddRangeAsync(invoices);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Facturas creadas: {Count}", invoices.Count);
        }
    }
}