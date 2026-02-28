//using DAM.Frontend.Core.Interfaces;
//using Microsoft.JSInterop;
//using Microsoft.Extensions.Logging;

//namespace DAM.Frontend.Core.Services
//{
//    /// <summary>
//    /// 🌓 Implementación de servicio de temas con detección de hora y emojis inteligentes
//    /// </summary>
//    public class ThemeService : IThemeService
//    {
//        private readonly IJSRuntime _jsRuntime;
//        private readonly IStorageService _storage;
//        private readonly ILogger<ThemeService> _logger;

//        private bool _isDarkMode;
//        private bool _userPreference;
//        private bool _isInitialized;

//        public event EventHandler<bool>? ThemeChanged;

//        public bool IsDarkMode => _isDarkMode;

//        public ThemeService(
//            IJSRuntime jsRuntime,
//            IStorageService storage,
//            ILogger<ThemeService> logger)
//        {
//            _jsRuntime = jsRuntime;
//            _storage = storage;
//            _logger = logger;
//        }

//        /// <summary>
//        /// 🌍 Inicializa el tema basado en: preferencia guardada > tema del sistema > hora del sistema
//        /// </summary>
//        public async Task InitializeThemeAsync()
//        {
//            if (_isInitialized) return;

//            try
//            {
//                // 1️⃣ Intentar cargar preferencia guardada
//                var savedTheme = await _storage.GetAsync<bool?>("user_theme_preference");

//                if (savedTheme.HasValue)
//                {
//                    _userPreference = savedTheme.Value;
//                    await SetThemeAsync(_userPreference, false);
//                    _logger.LogInformation("🎨 Tema cargado desde preferencia: {Theme}",
//                        _userPreference ? "Oscuro" : "Claro");
//                }
//                else
//                {
//                    // 2️⃣ Detectar tema del sistema con JavaScript
//                    var isDarkSystem = await DetectSystemThemeAsync();

//                    if (isDarkSystem.HasValue)
//                    {
//                        await SetThemeAsync(isDarkSystem.Value, false);
//                        _logger.LogInformation("🖥️ Tema basado en sistema: {Theme}",
//                            isDarkSystem.Value ? "Oscuro" : "Claro");
//                    }
//                    else
//                    {
//                        // 3️⃣ Fallback a hora del sistema
//                        var currentHour = DateTime.Now.Hour;
//                        var isNightTime = currentHour < 6 || currentHour >= 18;
//                        await SetThemeAsync(isNightTime, false);
//                        _logger.LogInformation("🕐 Tema basado en hora del sistema ({Hour}h): {Theme}",
//                            currentHour, isNightTime ? "Oscuro" : "Claro");
//                    }
//                }

//                _isInitialized = true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "❌ Error inicializando tema");
//                await SetThemeAsync(false, false);
//            }
//        }

//        /// <summary>
//        /// 🔍 Detecta el tema del sistema usando JavaScript
//        /// </summary>
//        private async Task<bool?> DetectSystemThemeAsync()
//        {
//            try
//            {
//                // Ejecutar JavaScript para detectar preferencia del sistema
//                var isDark = await _jsRuntime.InvokeAsync<bool>("eval", @"
//                    window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
//                ");

//                return isDark;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogWarning(ex, "⚠️ No se pudo detectar tema del sistema");
//                return null;
//            }
//        }

//        /// <summary>
//        /// 🔄 Cambiar tema manualmente
//        /// </summary>
//        public async Task ToggleThemeAsync()
//        {
//            await SetThemeAsync(!_isDarkMode, true);
//        }

//        /// <summary>
//        /// ⚙️ Establecer tema específico
//        /// </summary>
//        public async Task SetThemeAsync(bool isDark, bool savePreference = true)
//        {
//            if (_isDarkMode == isDark) return;

//            _isDarkMode = isDark;

//            // Aplicar tema con JavaScript
//            await ApplyThemeAsync(isDark);

//            // Guardar preferencia si es cambio manual
//            if (savePreference)
//            {
//                _userPreference = isDark;
//                await _storage.SetAsync("user_theme_preference", isDark);
//                _logger.LogInformation("💾 Preferencia de tema guardada: {Theme}",
//                    isDark ? "Oscuro" : "Claro");
//            }

//            // Notificar cambio
//            ThemeChanged?.Invoke(this, isDark);

//            // Actualizar emoji
//            await UpdateThemeEmojiAsync();
//        }

//        /// <summary>
//        /// 🎨 Aplica el tema usando JavaScript
//        /// </summary>
//        private async Task ApplyThemeAsync(bool isDark)
//        {
//            try
//            {
//                var themeClass = isDark ? "dark-theme" : "light-theme";
//                await _jsRuntime.InvokeVoidAsync("eval", $@"
//                    document.documentElement.classList.remove('dark-theme', 'light-theme');
//                    document.documentElement.classList.add('{themeClass}');
//                ");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "❌ Error aplicando tema");
//            }
//        }

//        /// <summary>
//        /// ⚙️ Sobrecarga para compatibilidad
//        /// </summary>
//        public async Task SetThemeAsync(bool isDark)
//        {
//            await SetThemeAsync(isDark, true);
//        }

//        /// <summary>
//        /// ☀️🌙 Emoji inteligente: INVERTIDO según contexto
//        /// </summary>
//        public string GetSmartThemeEmoji()
//        {
//            return _isDarkMode ? "☀️" : "🌙";
//        }

//        /// <summary>
//        /// 💬 Tooltip dinámico para botón de tema
//        /// </summary>
//        public string GetThemeTooltip()
//        {
//            var currentHour = DateTime.Now.Hour;
//            var timeOfDay = currentHour < 12 ? "mañana" : currentHour < 18 ? "tarde" : "noche";

//            return _isDarkMode
//                ? $"☀️ Cambiar a modo claro (son las {currentHour}:00, buena {timeOfDay})"
//                : $"🌙 Cambiar a modo oscuro (son las {currentHour}:00, buena {timeOfDay})";
//        }

//        /// <summary>
//        /// 🎬 Actualizar emoji en tiempo real
//        /// </summary>
//        private async Task UpdateThemeEmojiAsync()
//        {
//            try
//            {
//                await _jsRuntime.InvokeVoidAsync("updateThemeEmoji", GetSmartThemeEmoji());
//            }
//            catch { /* Ignorar */ }
//        }
//    }
//}

using DAM.Frontend.Core.Interfaces;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace DAM.Frontend.Core.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IStorageService _storage;
        private readonly ILogger<ThemeService> _logger;

        private bool _isDarkMode;
        private bool _userPreference;
        private bool _isInitialized;

        public event EventHandler<bool>? ThemeChanged;
        public bool IsDarkMode => _isDarkMode;

        public ThemeService(
            IStorageService storage,
            ILogger<ThemeService> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public async Task<bool?> GetSavedThemeAsync()
        {
            try
            {
                return await _storage.GetAsync<bool?>("user_theme_preference");
            }
            catch
            {
                return null;
            }
        }

        public async Task InitializeThemeAsync()
        {
            if (_isInitialized) return;

            try
            {
                var savedTheme = await GetSavedThemeAsync();

                if (savedTheme.HasValue)
                {
                    _userPreference = savedTheme.Value;
                    await SetThemeAsync(_userPreference, false);
                    _logger.LogInformation("🎨 Tema cargado de preferencia: {Theme}",
                        _userPreference ? "Oscuro" : "Claro");
                }
                else
                {
                    // El tema del sistema se maneja vía JS en el inicializador
                    // Por ahora usamos un valor por defecto
                    await SetThemeAsync(false, false);
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inicializando tema");
                await SetThemeAsync(false, false);
            }
        }

        public async Task ToggleThemeAsync()
        {
            await SetThemeAsync(!_isDarkMode, true);
        }

        public async Task SetThemeAsync(bool isDark, bool savePreference = true)
        {
            if (_isDarkMode == isDark) return;

            _isDarkMode = isDark;

            if (savePreference)
            {
                _userPreference = isDark;
                await _storage.SetAsync("user_theme_preference", isDark);
                _logger.LogInformation("💾 Preferencia guardada: {Theme}",
                    isDark ? "Oscuro" : "Claro");
            }

            ThemeChanged?.Invoke(this, isDark);
        }

        public async Task SetThemeAsync(bool isDark)
        {
            await SetThemeAsync(isDark, true);
        }

        public string GetSmartThemeEmoji()
        {
            return _isDarkMode ? "☀️" : "🌙";
        }

        public string GetThemeTooltip()
        {
            var currentHour = DateTime.Now.Hour;
            var timeOfDay = currentHour < 12 ? "mañana" : currentHour < 18 ? "tarde" : "noche";

            return _isDarkMode
                ? $"☀️ Cambiar a claro (son las {currentHour}:00, buena {timeOfDay})"
                : $"🌙 Cambiar a oscuro (son las {currentHour}:00, buena {timeOfDay})";
        }
    }
}