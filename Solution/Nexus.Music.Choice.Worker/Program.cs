using Nexus.Music.Choice.Domain;
using Nexus.Music.Choice.Spotify;
using Nexus.Music.Choice.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddSingleton<IClock, SystemClock>()
    .AddInteractionServices()
    .AddIPCWorker()
    .AddSpotifyPlayer();

var host = builder.Build();
host.Run();

public class SystemClock : IClock
{
    public DateTime Now => DateTime.UtcNow;
}