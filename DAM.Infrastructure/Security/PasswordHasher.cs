using BCrypt.Net;
using DAM.Core.Abstractions;
using DAM.Core.Validations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DAM.Infrastructure.Security;

/// <summary>
/// Implementación concreta de <see cref="IPasswordHasher"/> utilizando BCrypt.Net.
/// </summary>
/// <remarks>
/// <para>
/// <b>¿Por qué BCrypt?</b>
/// <list type="number">
/// <item><description><b>Adaptativo:</b> El factor de trabajo puede incrementarse con el tiempo</description></item>
/// <item><description><b>Salt automático:</b> Genera y almacena el salt en el propio hash</description></item>
/// <item><description><b>Resistente a GPU/FPGA:</b> Diseñado para ser computacionalmente costoso</description></item>
/// <item><description><b>Estándar industrial:</b> Ampliamente adoptado y auditado</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Evolución del Work Factor:</b>
/// <list type="bullet">
/// <item><description>2010: WorkFactor = 6 (64 iteraciones) - Obsoleto</description></item>
/// <item><description>2016: WorkFactor = 10 (1024 iteraciones) - Débil</description></item>
/// <item><description>2021: WorkFactor = 11 (2048 iteraciones) - Aceptable</description></item>
/// <item><description><b>2026: WorkFactor = 12 (4096 iteraciones) - Recomendado</b></description></item>
/// <item><description>2030: WorkFactor = 13 (8192 iteraciones) - Objetivo futuro</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Rendimiento en hardware moderno (2026):</b>
/// <list type="bullet">
/// <item><description>WF=12: ~250ms por hash (recomendado)</description></item>
/// <item><description>WF=13: ~500ms por hash (para alta seguridad)</description></item>
/// <item><description>WF=14: ~1000ms por hash (solo operaciones administrativas)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Registro en contenedor DI con factor de trabajo configurable:
/// <code>
/// // En Program.cs
/// builder.Services.AddSingleton&lt;IPasswordHasher&gt;(sp => 
/// {
///     // Leer de configuración o usar valor por defecto
///     var workFactor = builder.Configuration.GetValue&lt;int&gt;("BCrypt:WorkFactor", 12);
///     return new PasswordHasher(workFactor);
/// });
/// </code>
/// </example>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Factor de trabajo (costo) por defecto para BCrypt.
    /// Valor: 12 (4096 iteraciones) - Balance óptimo seguridad/rendimiento para 2026.
    /// </summary>
    private const int DefaultWorkFactor = 12;

    private readonly int _workFactor;
    private readonly bool _enforceStrongPasswords;
    private readonly ILogger<PasswordHasher>? _logger;
    private readonly IValidator<string> _passwordValidator;
    private readonly IValidator<string> _hashValidator;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="PasswordHasher"/>.
    /// </summary>
    /// <param name="settings">Configuración de BCrypt (opcional).</param>
    /// <param name="logger">Logger para auditoría (opcional).</param>
    /// <param name="passwordValidators">Colección de validadores de contraseña (opcional).</param>
    /// <param name="hashValidator">Validador de formato BCrypt (opcional).</param>
    public PasswordHasher(IOptions<BCryptSettings>? settings = null,
                          ILogger<PasswordHasher>? logger = null,
                          IEnumerable<IValidator<string>>? passwordValidators = null,
                          IValidator<string>? hashValidator = null)
    {
        var config = settings?.Value ?? new BCryptSettings();

        _workFactor = config.WorkFactor;
        _enforceStrongPasswords = config.EnforceStrongPasswords;

        if (_workFactor is < 4 or > 31)
            throw new ArgumentOutOfRangeException(nameof(_workFactor),
                "El factor de trabajo debe estar entre 4 y 31.");

        _logger = logger;

        // 🎯 AQUÍ USAMOS EL FLAG DE CONFIGURACIÓN
        if (_enforceStrongPasswords)
        {
            // Busca explícitamente StrongPasswordValidator
            _passwordValidator = passwordValidators?
                .FirstOrDefault(v => v is StrongPasswordValidator)
                ?? new StrongPasswordValidator();

            _logger?.LogDebug("Modo: Contraseñas fuertes requeridas");
        }
        else
        {
            // Busca cualquier validador o usa el básico
            _passwordValidator = passwordValidators?
                .FirstOrDefault()
                ?? new PasswordValidator();

            _logger?.LogDebug("Modo: Contraseñas básicas permitidas");
        }

        _hashValidator = hashValidator ?? new BcryptHashValidator();

        _logger?.LogDebug("PasswordHasher inicializado con WorkFactor = {WorkFactor}, Validador = {Validator}",
            _workFactor, _passwordValidator.GetType().Name);
    }

    /// <inheritdoc/>
    public string Hash(string password)
    {
        var validationResult = _passwordValidator.Validate(password);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger?.LogWarning("Validación de contraseña fallida: {Errors}", errors);

            throw new ArgumentException($"Contraseña inválida: {errors}", nameof(password));
        }

        try
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password, _workFactor);

            _logger?.LogDebug("Hash generado exitosamente. WorkFactor: {WorkFactor}", _workFactor);

            return hash;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al generar hash para contraseña");
            throw new InvalidOperationException("No se pudo procesar la contraseña.", ex);
        }
    }

    /// <inheritdoc/>
    public bool Verify(string password, string hash)
    {
        (bool flowControl, bool value) = ValidateFlow(password, hash);
        if (!flowControl)
        {
            return value;
        }

        try
        {
            var isValid = BCrypt.Net.BCrypt.Verify(password, hash);

            if (!isValid)
            {
                _logger?.LogWarning("Intento de verificación fallido");
            }

            return isValid;
        }
        catch (BCrypt.Net.SaltParseException ex)
        {
            _logger?.LogError(ex, "Formato de hash inválido");
            throw new FormatException("El hash proporcionado no tiene un formato válido.", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error durante verificación de contraseña");
            throw new InvalidOperationException("No se pudo verificar la contraseña.", ex);
        }
    }

    /// <summary>
    /// Valida los parámetros de entrada del método <see cref="Verify"/> antes de la verificación criptográfica.
    /// </summary>
    /// <param name="password">Contraseña en texto plano a validar.</param>
    /// <param name="hash">Hash BCrypt a validar.</param>
    /// <returns>
    /// Una tupla que indica si el flujo debe continuar y, en caso de fallo, el valor por defecto a retornar.
    /// <list type="bullet">
    /// <item><description><c>flowControl</c>: <see langword="true"/> si ambos parámetros son válidos, <see langword="false"/> en caso contrario.</description></item>
    /// <item><description><c>value</c>: Siempre <see langword="false"/> cuando hay fallo, <see langword="default"/> cuando es exitoso.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <b>Razones para NO lanzar excepciones aquí:</b>
    /// </para>
    /// <list type="number">
    /// <item><description>
    /// <b>Seguridad (Timing Attacks):</b> Lanzar excepción inmediatamente para credenciales inválidas
    /// revelaría información sobre por qué falló la autenticación, permitiendo ataques de enumeración.
    /// </description></item>
    /// <item><description>
    /// <b>Consistencia:</b> El método <see cref="Verify"/> retorna <see langword="false"/> 
    /// tanto para credenciales incorrectas como para formatos inválidos, manteniendo una superficie de ataque uniforme.
    /// </description></item>
    /// <item><description>
    /// <b>UX vs Seguridad:</b> Aunque queremos dar buena experiencia al usuario, en autenticación
    /// prima la seguridad sobre la claridad del error.
    /// </description></item>
    /// </list>
    /// <para>
    /// <b>Patrón aplicado:</b> Guard Clause temprana con retorno silencioso.
    /// </para>
    /// </remarks>
    private (bool flowControl, bool value) ValidateFlow(string password, string hash)
    {
        // Validación de contraseña
        var passwordValidation = _passwordValidator.Validate(password);
        if (!passwordValidation.IsValid)
        {
            _logger?.LogWarning("Contraseña inválida en verificación");
            return (flowControl: false, value: false); // No lanzamos excepción por seguridad (timing attack)
        }

        // Validación de hash
        var hashValidation = _hashValidator.Validate(hash);
        if (!hashValidation.IsValid)
        {
            _logger?.LogWarning("Hash con formato inválido");
            return (flowControl: false, value: false);
        }

        return (flowControl: true, value: default);
    }
}