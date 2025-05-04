namespace Nexus.Music.Choice.Domain.Services.Interfaces;

public interface ITokenStoreService
{
    Task SaveTokenAsync(TokenData token);
    Task<TokenData?> GetTokenAsync();
    Task DeleteTokenAsync();
}

public class TokenData
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime? ExpiresIn { get; set; }
}

public class AccessTokenChangedEventArgs
{
    public string ServiceName { get; set; }
    public string AccessToken { get; set; }
    public DateTime? ExpiresIn { get; set; }
}