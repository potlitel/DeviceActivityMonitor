using DAM.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 🎯 REGISTRO MODULAR DE SERVICIOS
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddSecurity(builder.Configuration, builder.Environment) // 🔐 JWT + IdentityService
    .AddRepositories()                                       // 📦 Repositorios
    .AddDomainServices()                                     // ⚙️ Servicios de dominio
    .AddCQRS()                                               // 📨 Handlers automáticos
    .AddValidation()                                         // ✅ FluentValidation
    .AddBCrypt(builder.Configuration)                        // 🔧 BCrypt settings
    .AddFastEndpointsWithSwagger()                           // 🚀 FastEndpoints + Swagger
    .AddHealthChecksWithChecks();                            // 🩺 Health checks

//builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// 🎯 PIPELINE DE LA APLICACIÓN
app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpointsPipeline();
app.UseSwaggerWithUI();

// 🩺 Health Checks
app.MapHealthChecksWithUI();

// 🌱 Inicialización de BD
await app.EnsureDatabaseCreatedAsync();

app.Run();
