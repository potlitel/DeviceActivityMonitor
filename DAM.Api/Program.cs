using DAM.Api.Features.Audit;
using DAM.Api.Infrastructure.Health;
using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.Audit;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Features.Invoices.Commands;
using DAM.Core.Interfaces;
//using DAM.Infrastructure.Caching.Decorators;
using DAM.Infrastructure.CQRS;
//using DAM.Infrastructure.Features.Audit;
using DAM.Infrastructure.Identity;
using DAM.Infrastructure.Persistence;
using DAM.Infrastructure.Repositories;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NSwag;

var builder = WebApplication.CreateBuilder(args);
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "Clave_Super_Secreta_Bancaria_2026_DAM";

// 3. Core & Business Services
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
//builder.Services.AddScoped<IInvoiceCalculator, InvoiceCalculator>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 4. CQRS Dispatcher
builder.Services.AddScoped<IDispatcher, InternalDispatcher>();

// 5. Registro automático de Handlers (Usando reflexión o manual)
//builder.Services.AddScoped<IQueryHandler<GetActivitiesQuery, PaginatedList<ActivityDto>>, GetActivitiesHandler>();
//builder.Services.AddScoped<ICommandHandler<CalculateInvoiceCommand, InvoiceResponse>, CalculateInvoiceHandler>();

// 1. Configuración de Seguridad
builder.Services.AddAuthenticationJwtBearer(s => s.SigningKey = jwtSecret); // Versión correcta
builder.Services.AddAuthorization();

//builder.Services.AddHealthChecks()
//    .AddCheck<StorageHealthCheck>("Almacenamiento")
//    .AddDbContextCheck<DeviceActivityDbContext>("Base de Datos")
//    .AddProcessAllocatedMemoryCheck(maximumMegabytesAllocated: 512, name: "Memoria RAM");

builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IPresenceRepository, PresenceRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IServiceEventRepository, ServiceEventRepository>();
//builder.Services.AddScoped<IAuditRepository, AuditRepository>();

// Registro del Handler Original
//builder.Services.AddScoped<GetAuditLogsHandler>();

// Registro del Decorador (La API pedirá IQueryHandler y recibirá el Cached)
//builder.Services.AddScoped<IQueryHandler<GetAuditLogsQuery, PaginatedList<AuditLogResponse>>>(sp =>
//{
//    var original = sp.GetRequiredService<GetAuditLogsHandler>();
//    var cache = sp.GetRequiredService<ICacheService>();
//    return new CachedGetAuditLogsHandler(original, cache);
//});

// 2. Swagger con soporte para JWT (Configuración para NSwag)
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "DAM API - Device Activity Monitor";
        s.Version = "v1";

        var scheme = new NSwag.OpenApiSecurityScheme
        {
            Description = "Introduzca el token JWT: Bearer {tu_token}",
            Name = "Authorization",
            In = OpenApiSecurityApiKeyLocation.Header, // Sintaxis específica de NSwag
            Type = OpenApiSecuritySchemeType.ApiKey     // Sintaxis específica de NSwag
        };

        s.AddSecurity("Bearer", scheme);
    };
});

builder.Services.AddFastEndpoints();

var app = builder.Build();

// 3. Middlewares
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
    // Si usas el AuditPreProcessor, se registra aquí:
    // c.Endpoints.Configurator = ep => ep.PreProcessors(Order.Before, new AuditPreProcessor());
});

app.UseSwaggerGen();

// Endpoint público para balanceadores (ligero)
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Endpoint detallado para administradores (protegido)
app.MapHealthChecks("/health/ready", HealthCheckExtensions.GetJsonOptions());

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DeviceActivityDbContext>();
        //DbInitializer.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al poblar la base de datos.");
    }
}

app.Run();