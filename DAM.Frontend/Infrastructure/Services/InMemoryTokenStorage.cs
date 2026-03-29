namespace DAM.Frontend.Infrastructure.Services;

/// <summary>
/// 💾 Almacenamiento temporal en memoria para el token durante prerendering
/// </summary>
public interface IInMemoryTokenStorage
{
    string? GetToken();
    void SetToken(string token);
    string? GetExpiry();
    void SetExpiry(string expiry);
    void Clear();
}

public class InMemoryTokenStorage : IInMemoryTokenStorage
{
    private string? _token;
    private string? _expiry;

    public string? GetToken() => _token;
    
    public void SetToken(string token) => _token = token;
    
    public string? GetExpiry() => _expiry;
    
    public void SetExpiry(string expiry) => _expiry = expiry;
    
    public void Clear()
    {
        _token = null;
        _expiry = null;
    }
}
