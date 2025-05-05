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

        services.AddHttpClient<ISpotifyApiService, SpotifyApiService>()
            .AddHttpMessageHandler(provider =>
                new AuthenticatedHttpClientHandler(provider.GetRequiredService<ISpotifyApiAuthenticationService>())
            )
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https://api.spotify.com/v1/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Nexus.Music.Choice/1.0");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            });

        return services
            .AddSingleton<ISpotifyTokenStoreService, SpotifyTokenStoreService>()
            .AddScoped<ISpotifyApiAuthenticationService, SpotifyAuthenticationService>()
            .AddScoped<IApiAuthenticationService>(sp => sp.GetRequiredService<ISpotifyApiAuthenticationService>())
            .AddSingleton<IMusicPlayerService, SpotifyMusicPlayerService>();
    }
}

public class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly IApiAuthenticationService _authService;

    public AuthenticatedHttpClientHandler(ISpotifyApiAuthenticationService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await _authService.WaitForAuthorizationAsync();

        return await base.SendAsync(request, cancellationToken);
    }
}