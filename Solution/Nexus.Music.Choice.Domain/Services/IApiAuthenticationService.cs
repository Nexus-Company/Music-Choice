namespace Nexus.Music.Choice.Domain.Services;

public interface IApiAuthenticationService
{
    public string Name { get; }
    string? GetAcessToken();
    Task<bool> CheckAuthenticationAsync();
    Task StartAuthenticationAsync();
}