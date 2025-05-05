using Newtonsoft.Json;
using Nexus.Music.Choice.Domain.Models;

namespace Nexus.Music.Choice.Spotify.Models;

public class SpotifyTrack : Track
{
    [JsonProperty("duration_ms")]
    public override int Duration { get; set; }
}