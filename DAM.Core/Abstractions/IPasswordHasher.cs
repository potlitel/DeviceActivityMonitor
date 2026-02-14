namespace DAM.Core.Abstractions;

/// <summary>
/// Define el contrato para el cifrado y validación de credenciales utilizando algoritmos 
/// de hashing seguros y adaptativos.
/// </summary>
/// <remarks>
/// <para>
/// Esta interfaz abstrae la implementación concreta del algoritmo de hashing, permitiendo:
/// <list type="bullet">
/// <item><description>Desacoplar la lógica de negocio de la implementación criptográfica</description></item>
/// <item><description>Facilitar pruebas unitarias mediante mocking</description></item>
/// <item><description>Migrar a algoritmos más seguros sin modificar el código cliente</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Consideraciones de seguridad:</b>
/// <list type="bullet">
/// <item><description>Nunca almacenar contraseñas en texto plano</description></item>
/// <item><description>Utilizar salt automático por usuario (BCrypt lo maneja internamente)</description></item>
/// <item><description>El factor de trabajo debe incrementarse periódicamente con la capacidad computacional</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Uso típico en servicios de autenticación:
/// <code>
/// public class AuthService
/// {
///     private readonly IPasswordHasher _hasher;
///     
///     public AuthService(IPasswordHasher hasher)
///     {
///         _hasher = hasher;
///     }
///     
///     public User Register(string email, string password)
///     {
///         var passwordHash = _hasher.Hash(password);
///         return new User(email, passwordHash);
///     }
///     
///     public bool ValidateCredentials(User user, string password)
///     {
///         return _hasher.Verify(password, user.PasswordHash);
///     }
/// }
/// </code>
/// </example>
public interface IPasswordHasher
{
    /// <summary>
    /// Genera un hash seguro a partir de una contraseña en texto plano utilizando 
    /// BCrypt con salt automático y factor de trabajo configurable.
    /// </summary>
    /// <param name="password">Contraseña en texto plano. No debe ser nula ni vacía.</param>
    /// <returns>Hash de 60 caracteres en formato Modular Crypt Format (MCF).</returns>
    /// <exception cref="ArgumentNullException">Lanzada cuando la contraseña es nula.</exception>
    /// <exception cref="ArgumentException">Lanzada cuando la contraseña está vacía o excede la longitud máxima.</exception>
    /// <remarks>
    /// <para>
    /// El hash generado incluye:
    /// <list type="bullet">
    /// <item><description>Algoritmo: bcrypt ($2a$)</description></item>
    /// <item><description>Factor de trabajo (costo)</description></item>
    /// <item><description>Salt de 16 bytes (128 bits) generado criptográficamente</description></item>
    /// <item><description>Hash de 24 bytes (192 bits)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Formato del hash: <c>$2a${costo}${salt}{hash}</c>
    /// Ejemplo: <c>$2a$12$K2X6J8Y5N2mQ9r7V3xW1uO5tZ2nL8pR4sT7vY9bA0cD3eF1gH2iJ3k</c>
    /// </para>
    /// </remarks>
    string Hash(string password);

    /// <summary>
    /// Verifica si una contraseña en texto plano coincide con un hash previamente generado.
    /// </summary>
    /// <param name="password">Contraseña en texto plano a verificar.</param>
    /// <param name="hash">Hash previamente generado por <see cref="Hash"/>.</param>
    /// <returns>
    /// <see langword="true"/> si la contraseña coincide con el hash; 
    /// <see langword="false"/> en caso contrario.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Lanzada cuando la contraseña o el hash son nulos.
    /// </exception>
    /// <exception cref="FormatException">
    /// Lanzada cuando el hash no tiene un formato válido de BCrypt.
    /// </exception>
    /// <remarks>
    /// <para>
    /// La verificación es tiempo-constante, lo que previene ataques de timing.
    /// Extrae automáticamente el salt y factor de trabajo del hash para la verificación.
    /// </para>
    /// <para>
    /// <b>Buenas prácticas:</b>
    /// <list type="bullet">
    /// <item><description>Nunca revelar qué parte falló (usuario vs contraseña)</description></item>
    /// <item><description>Registrar intentos fallidos para detección de ataques</description></item>
    /// <item><description>Considerar re-hash si el factor de trabajo es inferior al actual</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    bool Verify(string password, string hash);
}