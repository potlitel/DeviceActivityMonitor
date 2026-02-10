//using DAM.Core.Entities;
//using DAM.Core.Enums;
//using DAM.Infrastructure.Audit;
//using Microsoft.AspNetCore.Identity;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics.Tracing;
//using System.Linq;

//namespace DAM.Infrastructure.Persistence;

//public static class DbInitializer
//{
//    public static void Seed(DeviceActivityDbContext context)
//    {
//        context.Database.EnsureCreated();

//        if (context.Users.Any()) return; // Si ya hay datos, no hacer nada

//        // 1. USUARIOS (Password: P@ssw0rd2026)
//        var manager = new ApplicationUser
//        {
//            Id = Guid.NewGuid(),
//            Username = "admin_manager",
//            Email = "manager@dam.com",
//            PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd2026"),
//            Role = UserRole.Manager,
//            IsTwoFactorEnabled = true, // Simulamos que ya lo activó
//            TwoFactorSecret = "JBSWY3DPEHPK3PXP" // Secret de ejemplo
//        };

//        var worker = new ApplicationUser
//        {
//            Id = Guid.NewGuid(),
//            Username = "worker_user",
//            Email = "worker@dam.com",
//            PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd2026"),
//            Role = UserRole.Worker,
//            IsTwoFactorEnabled = false
//        };

//        context.Users.AddRange(manager, worker);

//        // 2. ACTIVIDADES DE DISPOSITIVOS (Historial de los últimos 7 días)
//        var activities = new List<DeviceActivity>();
//        for (int i = 1; i <= 20; i++)
//        {
//            activities.Add(new DeviceActivity
//            {
//                Id = Guid.NewGuid(),
//                SerialNumber = $"SN-2026-X{i:00}",
//                Status = i % 5 == 0 ? ActivityStatus.Completed : ActivityStatus.InProgress,
//                MegabytesCopied = 1024 * i,
//                MegabytesDeleted = 512 * (i / 2),
//                InsertedAt = DateTime.UtcNow.AddDays(-i).AddHours(i)
//            });
//        }
//        context.DeviceActivities.AddRange(activities);

//        // 3. EVENTOS DE SISTEMA (La Caja Negra)
//        context.ServiceEvents.AddRange(
//            new ServiceEvent { Level = EventLevel.Information, Source = "Watcher", Message = "Sistema de monitoreo iniciado correctamente.", TimestampUtc = DateTime.UtcNow.AddDays(-10) },
//            new ServiceEvent { Level = EventLevel.Warning, Source = "Watcher", Message = "Latencia detectada en disco USB 3.0", TimestampUtc = DateTime.UtcNow.AddDays(-5) },
//            new ServiceEvent { Level = EventLevel.Error, Source = "Persistence", Message = "Error de escritura temporal en SQLite (Database Locked)", TimestampUtc = DateTime.UtcNow.AddDays(-1) }
//        );

//        // 4. FACTURAS (Para las actividades completadas)
//        foreach (var act in activities.Where(a => a.Status == ActivityStatus.Completed))
//        {
//            context.Invoices.Add(new Invoice
//            {
//                Id = Guid.NewGuid(),
//                DeviceActivityId = act.Id,
//                InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{act.SerialNumber.Split('-').Last()}",
//                TotalAmount = (decimal)(act.MegabytesCopied * 0.05), // $0.05 por MB
//                GeneratedAt = act.InsertedAt.AddHours(2)
//            });
//        }

//        // 5. TRAZAS DE AUDITORÍA (Simulando actividad previa)
//        context.AuditLogs.AddRange(
//            new AuditLog { Username = "admin_manager", Action = "LOGIN_SUCCESS", Resource = "/api/identity/login", HttpMethod = "POST", IpAddress = "127.0.0.1", TimestampUtc = DateTime.UtcNow.AddHours(-5) },
//            new AuditLog { Username = "admin_manager", Action = "GENERATE_INVOICE", Resource = "/api/invoices/calculate", HttpMethod = "POST", IpAddress = "127.0.0.1", TimestampUtc = DateTime.UtcNow.AddHours(-2) }
//        );

//        context.SaveChanges();
//    }
//}