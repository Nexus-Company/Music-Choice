namespace Nexus.Music.Choice.Domain.Services.Interfaces;

public interface IApiAuthenticationService
{
    public string Name { get; }
    Task<bool> CheckAuthenticationAsync();
    Task StartAuthenticationAsync();
}