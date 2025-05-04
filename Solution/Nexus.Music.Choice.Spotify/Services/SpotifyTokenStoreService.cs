using Nexus.Music.Choice.Domain.Services;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Services.Interfaces;

namespace Nexus.Music.Choice.Spotify.Services;

internal class SpotifyTokenStoreService : BaseTokenStoreService, ITokenStoreService, ISpotifyTokenStoreService
{
    private const string Name = "Spotify";

    private event EventHandler<AccessTokenChangedEventArgs>? AccessTokenChanged;

    event EventHandler<AccessTokenChangedEventArgs> ISpotifyTokenStoreService.AccessTokenChanged
    {
        add
        {
            AccessTokenChanged += value;
        }

        remove
        {
            AccessTokenChanged -= value;
        }
    }

    private string? accessToken;

    async Task ITokenStoreService.DeleteTokenAsync()
    {
        await DeleteTokenAsync(Name);
    }

    async Task<TokenData?> ITokenStoreService.GetTokenAsync()
    {
        var token = await GetTokenAsync(Name);

        if (token != null)
            token.AccessToken = accessToken ?? string.Empty;
        
        return token;
    }

    async Task ITokenStoreService.SaveTokenAsync(TokenData token)
    {
        accessToken = token.AccessToken;

        AccessTokenChanged?.Invoke(this, new AccessTokenChangedEventArgs()
        {
            AccessToken = token.AccessToken,
            ExpiresIn = token.ExpiresIn,
            ServiceName = Name
        });

        await SaveTokenAsync(Name, token);
    }
}