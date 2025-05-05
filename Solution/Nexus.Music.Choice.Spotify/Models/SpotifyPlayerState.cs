using Newtonsoft.Json;
using Nexus.Music.Choice.Domain.Models;

namespace Nexus.Music.Choice.Spotify.Models;

public class SpotifyPlayerState : PlayerState
{
    [JsonProperty("item")]
    public SpotifyTrack? Track { get; set; }

    [JsonProperty("is_playing")]
    public override bool IsPlaying { get; set; }

    [JsonProperty("progress_ms")]
    public override int ProgressMilisseconds { get; set; }

    public SpotifyDevice? Device { get; set; }

    [JsonIgnore]
    public override Track? Item
    {
        get => Track;
        set => base.Item = value;
    }
}

public class SpotifyDevice
{
    public string? Id { get; set; }
    [JsonProperty("is_active")]
    public bool Active { get; set; }
    public string Name { get; set; }
    [JsonProperty("volume_percent")]
    public int? Volume { get; set; }
}