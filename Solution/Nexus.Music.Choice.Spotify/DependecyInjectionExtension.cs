using Microsoft.Extensions.DependencyInjection;
using Nexus.Music.Choice.Domain.Services;
using Nexus.Music.Choice.Spotify.Services;
using Nexus.Music.Choice.Spotify.Services.Interfaces;

namespace Nexus.Music.Choice.Spotify;

public static class DependecyInjectionExtension
{
    public static IServiceCollection AddSpotifyPlayer(this IServiceCollection services)
    {
        services
            .AddHttpClient<IApiAuthenticationService, SpotifyAuthenticationService>();

        //services
        //    .AddHttpClient<IMusicPlayerService, SpotifyMusicPlayerService>(client =>
        //    {
        //        client.BaseAddress = new Uri("https://api.spotify.com/v1/");
        //        client.DefaultRequestHeaders.Add("Accept", "application/json");
        //    });

        return services.AddSingleton<IMusicPlayerService, SpotifyMusicPlayerService>();
    }

}