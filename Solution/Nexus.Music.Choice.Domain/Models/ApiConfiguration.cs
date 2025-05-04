namespace Nexus.Music.Choice.Domain.Models;

public abstract class ApiConfiguration
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string[] Scopes { get; set; }
    public string RedirectUri { get; set; }
}