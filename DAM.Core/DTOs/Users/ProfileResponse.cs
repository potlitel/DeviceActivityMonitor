namespace DAM.Core.DTOs.Users;

/// <summary>
/// Respuesta completa del perfil de usuario con preferencias y claims.
/// </summary>
/// <remarks>
/// <para>
/// Proporciona una vista completa del usuario para interfaces administrativas
/// y paneles de configuración personalizada.
/// </para>
/// <para>
/// <b>Seguridad:</b> Los claims expuestos son seleccionados cuidadosamente
/// para no revelar información sensible como tokens o hashes.
/// </para>
/// </remarks>
/// <param name="Id">Identificador único del usuario.</param>
/// <param name="Username">Nombre de usuario.</param>
/// <param name="Email">Correo electrónico.</param>
/// <param name="Role">Rol del usuario en el sistema.</param>
/// <param name="IsTwoFactorEnabled">Indica si 2FA está activado.</param>
/// <param name="Claims">Lista de claims asociados al usuario.</param>
/// <param name="Preferences">Preferencias de personalización.</param>
public record ProfileResponse(
    Guid Id,
    string Username,
    string Email,
    string Role, // "Manager" o "Worker"
    bool IsTwoFactorEnabled,
    List<KeyValuePair<string, string>> Claims,
    UserPreferences Preferences
);

/// <summary>
/// Preferencias de personalización del usuario.
/// </summary>
/// <remarks>
/// Almacenadas típicamente como JSON en la base de datos para flexibilidad.
/// </remarks>
/// <param name="Theme">Tema de interfaz (Light/Dark/System).</param>
/// <param name="Language">Código de idioma (es-ES, en-US, etc.).</param>
public record UserPreferences(
    string Theme = "Light",
    string Language = "es-ES"
);