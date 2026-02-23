// 📁 DAM.Frontend/Infrastructure/Services/StorageService.cs
using DAM.Frontend.Core.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;

namespace DAM.Frontend.Infrastructure.Services;

/// <summary>
/// 💾 Implementación de almacenamiento local seguro con ProtectedBrowserStorage
/// </summary>
public class StorageService : IStorageService
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ILogger<StorageService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public StorageService(
        ProtectedLocalStorage localStorage,
        ILogger<StorageService> logger)
    {
        _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clave no puede estar vacía", nameof(key));

        try
        {
            _logger.LogDebug("📖 Leyendo de storage: {Key}", key);

            var result = await _localStorage.GetAsync<string>(key);

            if (!result.Success || string.IsNullOrEmpty(result.Value))
            {
                _logger.LogDebug("🔍 No se encontró valor para {Key}", key);
                return default;
            }

            try
            {
                var value = JsonSerializer.Deserialize<T>(result.Value, _jsonOptions);
                _logger.LogDebug("✅ Valor recuperado para {Key}: {ValueType}", key, typeof(T).Name);
                return value;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "❌ Error deserializando {Key} como {ValueType}", key, typeof(T).Name);

                // Intentar recuperar como string si T es string
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)result.Value;
                }

                return default;
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            // Error esperado durante prerendering - ignorar silenciosamente
            _logger.LogDebug("⚠️ Intento de acceso a storage durante prerendering: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error recuperando {Key} del storage", key);
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clave no puede estar vacía", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value), "No se puede guardar un valor nulo");

        try
        {
            _logger.LogDebug("💾 Guardando en storage: {Key}", key);

            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _localStorage.SetAsync(key, json);

            _logger.LogDebug("✅ Valor guardado para {Key}: {ValueType}", key, typeof(T).Name);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            // Error esperado durante prerendering - registrar pero no fallar
            _logger.LogWarning("⚠️ Intento de guardar en storage durante prerendering: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error guardando {Key} en storage", key);
            throw; // Relanzar porque es una operación crítica
        }
    }

    //public async Task SetAsync<T>(string key, T value)
    //{
    //    if (string.IsNullOrWhiteSpace(key))
    //        throw new ArgumentException("La clave no puede estar vacía", nameof(key));

    //    if (value == null)
    //        throw new ArgumentNullException(nameof(value), "No se puede guardar un valor nulo");

    //    try
    //    {
    //        _logger.LogDebug("💾 Guardando en storage: {Key} (Tipo: {Type})", key, typeof(T).Name);

    //        string stringValue;

    //        // ✅ CASO 1: Ya es string
    //        if (value is string str)
    //        {
    //            stringValue = str;
    //            _logger.LogDebug("📝 Valor directo string, longitud: {Length}", str.Length);
    //        }
    //        // ✅ CASO 2: Es otro tipo, serializar
    //        else
    //        {
    //            stringValue = JsonSerializer.Serialize(value, _jsonOptions);
    //            _logger.LogDebug("📄 Serializado a JSON: {JsonLength} caracteres", stringValue.Length);
    //        }

    //        // 🚨 VERIFICAR QUE NO ESTÉ VACÍO
    //        if (string.IsNullOrEmpty(stringValue))
    //        {
    //            _logger.LogError("❌ El valor serializado está vacío para {Key}", key);
    //            return;
    //        }

    //        await _localStorage.SetAsync(key, stringValue);

    //        // ✅ VERIFICACIÓN OPCIONAL
    //        var verify = await _localStorage.GetAsync<string>(key);
    //        _logger.LogDebug("✅ Verificación: {Result}", verify.Success ? "OK" : "FALLÓ");
    //    }
    //    catch (InvalidOperationException ex) 
    //    {
    //        _logger.LogWarning("⚠️ Intento de guardar en storage durante prerendering: {Key}", key);
    //    }
    //    catch (JsonException ex)
    //    {
    //        _logger.LogError(ex, "❌ Error de serialización JSON para {Key}", key);
    //        throw new InvalidOperationException($"No se pudo serializar el valor para {key}", ex);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "❌ Error guardando {Key} en storage", key);
    //        throw;
    //    }
    //}

    /// <inheritdoc/>
    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clave no puede estar vacía", nameof(key));

        try
        {
            _logger.LogDebug("🗑️ Eliminando del storage: {Key}", key);
            await _localStorage.DeleteAsync(key);
            _logger.LogDebug("✅ Eliminado {Key} del storage", key);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            _logger.LogWarning("⚠️ Intento de eliminar de storage durante prerendering: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error eliminando {Key} del storage", key);
        }
    }

    /// <inheritdoc/>
    public async Task ClearAsync()
    {
        try
        {
            _logger.LogDebug("🧹 Limpiando storage...");
            await RemoveAsync("auth_token");
            await RemoveAsync("auth_refresh_token");
            await RemoveAsync("auth_expiry");
            await RemoveAsync("user_profile");
            await RemoveAsync("user_theme_preference");
            await RemoveAsync("auto_theme_detected");
            _logger.LogInformation("✅ Storage limpiado completamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error limpiando storage");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ContainsKeyAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clave no puede estar vacía", nameof(key));

        try
        {
            var result = await _localStorage.GetAsync<string>(key);
            return result.Success && !string.IsNullOrEmpty(result.Value);
        }
        catch (InvalidOperationException) when (!OperatingSystem.IsBrowser())
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error verificando existencia de {Key}", key);
            return false;
        }
    }
}