/// <summary>
/// Excepción lanzada cuando la configuración del sistema (appsettings.json) es inválida o incompleta.
/// </summary>
public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message) { }
}