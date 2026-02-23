using DAM.Frontend.Core.Interfaces;
using DAM.Frontend.Infrastructure.Authentication;
using DAM.Frontend.Infrastructure.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;
using MudBlazor.Services;

namespace DAM.Frontend.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 📦 Storage
            services.AddScoped<ProtectedLocalStorage>();
            services.AddScoped<IStorageService, StorageService>();

            // 🌐 HTTP Client
            services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["Api:BaseUrl"] ??
                    "https://localhost:7156/api/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "DAM.Frontend/1.0");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // 🔐 Auth
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<AuthenticationStateProvider, CustomAuthProvider>();

            return services;
        }

        public static IServiceCollection AddMudServicesWithConfiguration(
            this IServiceCollection services)
        {
            services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
                config.SnackbarConfiguration.PreventDuplicates = true;
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 3000;
                config.SnackbarConfiguration.HideTransitionDuration = 200;
                config.SnackbarConfiguration.ShowTransitionDuration = 200;
            });

            return services;
        }
    }
}
