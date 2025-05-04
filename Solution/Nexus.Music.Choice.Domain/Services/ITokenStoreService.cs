namespace Nexus.Music.Choice.Domain.Services;

public interface ITokenStoreService
{
    Task SaveTokenAsync(string serviceName, TokenData token);
    Task<TokenData?> GetTokenAsync(string serviceName);
    Task DeleteTokenAsync(string serviceName);
}

public class TokenData
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime? ExpiresIn { get; set; }
}