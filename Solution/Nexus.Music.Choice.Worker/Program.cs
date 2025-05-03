using Nexus.Music.Choice.Spotify;
using Nexus.Music.Choice.Worker.Base.Dispatcher;
using Nexus.Music.Choice.Worker.Interfaces;
using Nexus.Music.Choice.Worker.PipeHandler;
using Nexus.Music.Choice.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddSingleton<IEventDispatcher<PipeWriter>, PipeEventDispatcher>()
    .AddSingleton<IEventDispatcher<IStreamWriter>>(sp => sp.GetRequiredService<IEventDispatcher<PipeWriter>>())
    .AddSpotifyPlayer()
    .AddSingleton(new InteractContext())
    .AddSingleton<IInteractionService, InteractionService>()
    .AddSingleton<ICommandDispatcher<PipeReader>, PipeCommandDispatcher>()
    .AddSingleton<ICommandDispatcher<IStreamReader>>(sp => sp.GetRequiredService<ICommandDispatcher<PipeReader>>())
   .AddSingleton<IPipeConnectionHandler, PipeConnectionHandler>()            // Verifique se todas as dependências de PipeConnectionHandler estão registradas
   .AddHostedService<PipeWorker>();                                          // Verifique se PipeWorker tem todas as dependências resolvidas

var host = builder.Build();
host.Run();


