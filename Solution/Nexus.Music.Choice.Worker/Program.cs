using Nexus.Music.Choice.Worker.Services;
using Nexus.Music.Choice.Worker.Workers.Handlers;
using Nexus.Music.Choice.Worker.Workers.MusicPipe;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddSingleton(new InteractContext())
    .AddSingleton<IInteractionService, InteractionService>()
    .AddSingleton<IMusicPipeConnectionHandler, MusicPipeConnectionHandler>()
    .AddHostedService<MusicPipeWorker>();

var host = builder.Build();
host.Run();
