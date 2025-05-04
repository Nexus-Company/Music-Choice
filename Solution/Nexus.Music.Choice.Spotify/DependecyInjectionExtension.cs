using Microsoft.Extensions.DependencyInjection;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Services;
using Nexus.Music.Choice.Spotify.Services.Interfaces;

namespace Nexus.Music.Choice.Spotify;

public static class DependecyInjectionExtension
{
    public static IServiceCollection AddSpotifyPlayer(this IServiceCollection services)
    {
        services
            .AddHttpClient<SpotifyAuthenticationService>();

        services
            .AddHttpClient<ISpotifyApiService, SpotifyApiService>(client =>
            {
                client.BaseAddress = new Uri("https://api.spotify.com/v1/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Nexus.Music.Choice/1.0");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            });

        return services
            .AddSingleton<ISpotifyTokenStoreService, SpotifyTokenStoreService>()
            .AddScoped<IApiAuthenticationService, SpotifyAuthenticationService>()
            .AddSingleton<IMusicPlayerService, SpotifyMusicPlayerService>();
    }

}