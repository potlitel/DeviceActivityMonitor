// 📁 DAM.Api/Extensions/ApplicationBuilderExtensions.cs
using DAM.Api.Infrastructure.Auth;
using DAM.Api.Infrastructure.Health;
using DAM.Infrastructure.Persistence;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace DAM.Api.Extensions;

/// <summary>
/// Métodos de extensión para configurar el pipeline de la aplicación.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 🚀 Configura el pipeline de FastEndpoints con middleware personalizado.
    /// </summary>
    public static IApplicationBuilder UseFastEndpointsPipeline(this IApplicationBuilder app)
    {
        app.UseFastEndpoints(c =>
        {
            c.Endpoints.RoutePrefix = "api";
            c.Endpoints.Configurator = ep =>
                ep.PreProcessors(Order.Before, new AuditPreProcessor());
        });

        return app;
    }

    /// <summary>
    /// 📖 Configura Swagger UI con título personalizado.
    /// </summary>
    public static IApplicationBuilder UseSwaggerWithUI(this IApplicationBuilder app)
    {
        app.UseSwaggerGen(uiConfig: u =>
        {
            u.CustomInlineStyles = """
                .swagger-ui .topbar { background-color: #2c3e50; }
                .swagger-ui .info .title { color: #2c3e50; font-weight: bold; }
                """;
        });

        // 🎯 REDIRECCIÓN AUTOMÁTICA A SWAGGER
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/")
            {
                context.Response.Redirect("/swagger");
                return;
            }
            await next();
        });

        return app;
    }

    /// <summary>
    /// 🩺 Configura endpoints de Health Checks con formato JSON personalizado.
    /// </summary>
    public static IEndpointRouteBuilder MapHealthChecksWithUI(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = report.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    uptime = Environment.TickCount64 / 1000
                };
                await context.Response.WriteAsJsonAsync(response);
            }
        });

        endpoints.MapHealthChecks("/health/ready", HealthCheckExtensions.GetJsonOptions());

        return endpoints;
    }

    /// <summary>
    /// 🌱 Inicializa la base de datos con migraciones y datos semilla.
    /// </summary>
    public static async Task<IApplicationBuilder> EnsureDatabaseCreatedAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<DeviceActivityDbContext>();
            var logger = services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("🔄 Aplicando migraciones pendientes...");
            await context.Database.MigrateAsync();
            logger.LogInformation("✅ Migraciones aplicadas correctamente");

            await DbInitializer.SeedAsync(context, logger);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "❌ Error durante la inicialización de la base de datos");
        }

        return app;
    }
}