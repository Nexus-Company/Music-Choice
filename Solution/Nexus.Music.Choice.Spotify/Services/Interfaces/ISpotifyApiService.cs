using Nexus.Music.Choice.Domain.Services.Interfaces;

namespace Nexus.Music.Choice.Spotify.Services.Interfaces;

public interface ISpotifyApiService : IApiService
{
}

public interface ISpotifyTokenStoreService : ITokenStoreService
{
    internal event EventHandler<AccessTokenChangedEventArgs> AccessTokenChanged;
}