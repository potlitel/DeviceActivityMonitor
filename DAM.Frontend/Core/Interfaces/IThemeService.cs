// 📁 DAM.Frontend/Core/Interfaces/IThemeService.cs
public interface IThemeService
{
    bool IsDarkMode { get; }
    event EventHandler<bool> ThemeChanged;

    Task InitializeThemeAsync();
    Task ToggleThemeAsync();
    Task SetThemeAsync(bool isDark, bool savePreference = true);
    Task<bool?> GetSavedThemeAsync(); // Nuevo método
    string GetSmartThemeEmoji();
    string GetThemeTooltip();
}