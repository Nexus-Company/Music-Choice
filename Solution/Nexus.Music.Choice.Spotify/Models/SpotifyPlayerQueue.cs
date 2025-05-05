namespace Nexus.Music.Choice.Spotify.Models;

public class SpotifyPlayerQueue
{
    public SpotifyTrack? Current { get; set; }

    public IEnumerable<SpotifyTrack> Queue { get; set; }
}