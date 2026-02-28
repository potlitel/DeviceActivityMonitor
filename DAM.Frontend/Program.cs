// 📁 DAM.Frontend/Program.cs (ACTUALIZADO)
using DAM.Frontend.Components;
using DAM.Frontend.Core.Interfaces;
using DAM.Frontend.Core.Services;
using DAM.Frontend.Infrastructure.Extensions;
using DAM.Frontend.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// 🎨 MudBlazor UI
builder.Services.AddMudServicesWithConfiguration();

// 🌓 Theme Service
//builder.Services.AddScoped<MudThemeProvider>();
builder.Services.AddScoped<IThemeService, ThemeService>();

// 📦 Servicios de aplicación
builder.Services.AddApplicationServices(builder.Configuration);

// 🔐 Autenticación
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/auth/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // 🚨 EVITAR REDIRECCIONES INFINITAS
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                // No redirigir si ya estamos en login
                if (context.Request.Path.StartsWithSegments("/auth/login"))
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Manager", policy => policy.RequireRole("Manager"));
    options.AddPolicy("Worker", policy => policy.RequireRole("Manager", "Worker"));
});

// ⚡ Blazor (estructura nueva)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// 📌 Mapeo para estructura nueva
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();