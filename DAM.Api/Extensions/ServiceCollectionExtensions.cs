using DAM.Api.Infrastructure.Auth;
using DAM.Api.Infrastructure.Health;
using DAM.Core.Abstractions;
using DAM.Core.Interfaces;
using DAM.Core.Repositories;
using DAM.Core.Validations;
using DAM.Infrastructure.Caching;
using DAM.Infrastructure.CQRS;
using DAM.Infrastructure.Features.DeviceActivity;
using DAM.Infrastructure.Health;
using DAM.Infrastructure.Identity;
using DAM.Infrastructure.Persistence;
using DAM.Infrastructure.Repositories;
using DAM.Infrastructure.Security;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace DAM.Api.Extensions;

/// <summary>
/// Métodos de extensión para configurar los servicios de la API de forma modular y limpia.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 🏗️ Registra la infraestructura base: DbContext, Caché, etc.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddDbContext<DeviceActivityDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    /// <summary>
    /// 🔐 Registra servicios de autenticación, autorización y seguridad.
    /// </summary>
    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // 🚨 1. Cargar y validar configuración JWT
        var jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>() ?? new JwtSettings();

        if (!jwtSettings.IsValid())
        {
            throw new ConfigurationException($"""
            ❌ Configuración JWT inválida. Verifica appsettings.json:
            - Secret: {(string.IsNullOrEmpty(jwtSettings.Secret) ? "❌ NO CONFIGURADO" : "✅ CONFIGURADO")}
            - Secret Length: {jwtSettings.Secret?.Length ?? 0} caracteres (mínimo 32)
            - Issuer: {jwtSettings.Issuer}
            - Audience: {jwtSettings.Audience}
            - AccessTokenExpiryMinutes: {jwtSettings.AccessTokenExpiryMinutes}
            """);
        }

        // ✅ 2. Registrar configuración JWT
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton(jwtSettings);

        // ✅ 3. Configurar autenticación JWT (NO usar AddAuthenticationJwtBearer de FastEndpoints)
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = !environment.IsDevelopment();

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes),

                RequireExpirationTime = true,
                RequireSignedTokens = true,

                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = ClaimTypes.Role
            };

            // 🚨 Eventos para debugging
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    logger.LogError(context.Exception, "❌ Autenticación JWT fallida");

                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }

                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    logger.LogWarning("🔐 Challenge JWT: {Error}, {Description}",
                        context.Error, context.ErrorDescription);

                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    var userId = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                    logger.LogDebug("✅ Token válido para usuario: {UserId}", userId);

                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        // ✅ 4. Registrar servicios de seguridad
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }

    /// <summary>
    /// 📦 Registra repositorios y Unit of Work.
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IPresenceRepository, PresenceRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IServiceEventRepository, ServiceEventRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    /// <summary>
    /// ⚙️ Registra servicios de dominio y lógica de negocio.
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddScoped<IInvoiceCalculator, FixedPriceInvoiceCalculator>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }

    /// <summary>
    /// 📨 Registra CQRS Dispatcher y Handlers AUTOMÁTICAMENTE usando Reflection
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🎯 Este método escanea TODOS los assemblies de la solución y registra:</b>
    /// <list type="bullet">
    /// <item><description><see cref="IDispatcher"/> - InternalDispatcher (Singleton/Scoped)</description></item>
    /// <item><description><see cref="ICommandHandler{TCommand, TResponse}"/> - Todos los command handlers</description></item>
    /// <item><description><see cref="IQueryHandler{TQuery, TResponse}"/> - Todos los query handlers</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>🔍 Estrategia de escaneo:</b>
    /// 1️⃣ Assembly.GetExecutingAssembly() - El assembly actual (DAM.Core)
    /// 2️⃣ AppDomain.CurrentDomain.GetAssemblies() - Todos los assemblies cargados
    /// 3️⃣ Assembly.GetEntryAssembly() - El assembly de entrada (DAM.API)
    /// </list>
    /// </para>
    /// <para>
    /// <b>⚠️ IMPORTANTE:</b> Los handlers DEBEN estar en assemblies referenciados por DAM.API
    /// </para>
    /// </remarks>
    public static IServiceCollection AddCQRS(this IServiceCollection services)
    {
        // 1️⃣ Registrar el Dispatcher
        services.AddScoped<IDispatcher, InternalDispatcher>();

        // 2️⃣ Obtener TODOS los assemblies relevantes
        var assemblies = new List<Assembly>
    {
        Assembly.GetExecutingAssembly(),
        Assembly.GetEntryAssembly()!,    
        typeof(GetActivitiesHandler).Assembly, 
        typeof(Core.Abstractions.ICommandHandler<,>).Assembly
    };

        // 3️⃣ Eliminar duplicados
        assemblies = assemblies.Distinct().ToList();

        // 4️⃣ Buscar TODOS los tipos que implementan ICommandHandler<,> o IQueryHandler<,>
        var handlerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && (
                    i.GetGenericTypeDefinition() == typeof(Core.Abstractions.ICommandHandler<,>) ||
                    i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                )))
            .ToList();

        if (!handlerTypes.Any())
        {
            // ⚠️ Fallback: Buscar en todos los assemblies cargados
            handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType && (
                        i.GetGenericTypeDefinition() == typeof(Core.Abstractions.ICommandHandler<,>) ||
                        i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                    )))
                .ToList();
        }

        // 5️⃣ Registrar cada handler en todas sus interfaces
        foreach (var handler in handlerTypes)
        {
            var interfaces = handler.GetInterfaces()
                .Where(i => i.IsGenericType && (
                    i.GetGenericTypeDefinition() == typeof(Core.Abstractions.ICommandHandler<,>) ||
                    i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                ));

            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, handler);

                // 🎯 Logging para debugging (quitar en producción)
#if DEBUG
                Console.WriteLine($"✅ Registrado: {@interface.Name} -> {handler.Name}");
#endif
            }
        }

        // 6️⃣ Estadísticas de registro
        var commandHandlers = handlerTypes.Count(t =>
            t.GetInterfaces().Any(i => i.GetGenericTypeDefinition() == typeof(Core.Abstractions.ICommandHandler<,>)));

        var queryHandlers = handlerTypes.Count(t =>
            t.GetInterfaces().Any(i => i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)));

#if DEBUG
        Console.WriteLine($"📊 CQRS Registry Summary:");
        Console.WriteLine($"   📨 Command Handlers: {commandHandlers}");
        Console.WriteLine($"   🔍 Query Handlers: {queryHandlers}");
        Console.WriteLine($"   📦 Total Handlers: {handlerTypes.Count}");
#endif

        return services;
    }

    /// <summary>
    /// ✅ Registra validadores de FluentValidation.
    /// </summary>
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }

    /// <summary>
    /// 🔧 Configura BCrypt con IOptions.
    /// </summary>
    public static IServiceCollection AddBCrypt(this IServiceCollection services, IConfiguration configuration)
    {
        var bcryptSettings = configuration
            .GetSection("BCrypt")
            .Get<BCryptSettings>() ?? new BCryptSettings();

        services.Configure<BCryptSettings>(configuration.GetSection("BCrypt"));
        services.AddSingleton(bcryptSettings);

        return services;
    }

    /// <summary>
    /// 🚀 Configura FastEndpoints y Swagger.
    /// </summary>
    public static IServiceCollection AddFastEndpointsWithSwagger(this IServiceCollection services)
    {
        services.AddFastEndpoints();

        services.SwaggerDocument(o =>
        {
            // 🎯 ESTO ES CRÍTICO - EVITA DUPLICADOS
            o.EnableJWTBearerAuth = true;
            o.DocumentSettings = s =>
            {
                s.Title = "DAM API - Device Activity Monitor";
                s.Version = "v1";
                s.Description = """
                    # 🖥️ DAM - Device Activity Monitor API
                    
                    ## 📋 Descripción del Sistema
                    API RESTful para el **monitoreo centralizado de dispositivos de almacenamiento USB** en entornos empresariales.
                    
                    ### 🎯 Propósito
                    Esta API expone los datos recolectados por el **Servicio de Monitoreo en Segundo Plano (Windows Service)**,
                    permitiendo la visualización, auditoría y gestión de:
                    
                    * 📱 **Actividades de dispositivos** - Ciclo completo inserción/extracción
                    * 👤 **Eventos de presencia** - Detección de dispositivos conectados
                    * 📊 **Facturación** - Cálculo automático por consumo de almacenamiento
                    * 🔐 **Seguridad** - Autenticación 2FA y control de acceso basado en roles
                    * 📝 **Auditoría** - Trazabilidad completa de operaciones
                    
                    ### 🏗️ Arquitectura
                    * **CQRS + Mediator** - Separación clara de responsabilidades
                    * **Repository Pattern** - Abstracción de persistencia
                    * **FluentValidation** - Validaciones declarativas
                    * **JWT Bearer** - Autenticación stateless
                    * **BCrypt** - Hashing adaptativo de contraseñas
                    
                    ### 🔒 Roles de Seguridad
                    | Rol | Descripción |
                    |-----|-------------|
                    | `Manager` | Acceso total a auditoría, facturación y gestión |
                    | `Worker` | Consulta básica de actividades y presencia |
                    
                    ### 📌 Notas de Versión
                    > **v1.0.0** - Implementación inicial con soporte completo para monitoreo de dispositivos
                    """;

                // 📋 DESHABILITAR TAGS AUTOMÁTICOS
                s.OperationProcessors.Add(new TagsOperationProcessor());

                // 🎯 CONTACTO - QUITAR COMENTARIO
                //s.Contact = new NSwag.OpenApiContact
                //{
                //    Name = "Equipo DAM - Arquitectura de Software",
                //    Email = "arquitectura@dam.com",
                //    Url = "https://dam.internal/architecture"
                //};

                s.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,  // 👈 OBLIGATORIO
                    Name = "Authorization",                    // 👈 Header name
                    In = OpenApiSecurityApiKeyLocation.Header, // 👈 En el header
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = """
                    🔐 **Autenticación JWT**
                    
                    **Instrucciones:**

                    1️⃣ Haz login en `/auth/login`.
                    
                    2️⃣ Copia el token de la respuesta.
                    
                    3️⃣ Pega aquí: `Bearer eyJhbGciOiJIUzI1NiIs...`.
                    
                    ⚠️ **IMPORTANTE:** Incluye la palabra "Bearer" antes del token.
                    
                    ✅ Ejemplo correcto:
                    ```
                    Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
                    ```
                    """
                });

                s.OperationProcessors.Add(new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("Bearer"));

            };
        });

        return services;
    }

    // 🔧 PROCESADOR PERSONALIZADO PARA LIMPIAR TAGS AUTOMÁTICOS
    public class TagsOperationProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            // Eliminar tags automáticos generados por FastEndpoints/NSwag
            if (context.OperationDescription.Operation.Tags?.Any() == true)
            {
                // 🎯 CONSERVAR SOLO TAGS PERSONALIZADOS (con emojis)
                var customTags = context.OperationDescription.Operation.Tags
                    .Where(t => t.Contains("📊") ||
                               t.Contains("📋") ||
                               t.Contains("🔐") ||
                               t.Contains("👤") ||
                               t.Contains("💰") ||
                               t.Contains("📱") ||
                               t.Contains("⚙️") ||
                               t.Contains("🔧"))
                    .ToList();

                if (customTags.Any())
                {
                    context.OperationDescription.Operation.Tags = customTags;
                }
                else
                {
                    // Si no hay tags personalizados, limpiar todo
                    context.OperationDescription.Operation.Tags = new List<string>();
                }
            }

            return true;
        }

        //public Task<bool> ProcessAsync(OperationProcessorContext context)
        //{
            
        //}
    }

    /// <summary>
    /// 🩺 Configura Health Checks.
    /// </summary>
    public static IServiceCollection AddHealthChecksWithChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<StorageHealthCheck>("💾 Almacenamiento")
            .AddDbContextCheck<DeviceActivityDbContext>("🗄️ Base de Datos");
            //.AddProcessAllocatedMemoryCheck(maximumMegabytesAllocated: 512, name: "🧠 Memoria RAM");

        return services;
    }

    /// <summary>
    /// Configura las políticas de Intercambio de Recursos de Origen Cruzado (CORS).
    /// </summary>
    /// <param name="services">Colección de servicios de IServiceCollection.</param>
    /// <param name="configuration">Configuración de la aplicación para extraer orígenes permitidos.</param>
    /// <returns>La colección de servicios configurada.</returns>
    /// <remarks>
    /// 🛡️ <b>Seguridad:</b> En entornos de producción, evite el uso de 'AllowAnyOrigin'.
    /// Esta implementación permite la comunicación fluida con el Frontend asegurando que los headers 
    /// de autorización (JWT) no sean bloqueados por el navegador.
    /// </remarks>
    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DAMPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }
}