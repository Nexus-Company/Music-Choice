using Microsoft.Extensions.DependencyInjection;
using Nexus.Music.Choice.Domain;

namespace Nexus.Music.Choice.Spotify;

public static class DependecyInjectionExtension
{
    public static IServiceCollection AddSpotifyPlayer(this IServiceCollection services)
    {
        services.AddSingleton<IMusicPlayerService, SpotifyMusicPlayerService>();
        return services;
    }
}