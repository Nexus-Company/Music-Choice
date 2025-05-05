using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Worker.Base.Dispatcher;
using Nexus.Music.Choice.Worker.Interfaces;
using Nexus.Music.Choice.Worker.PipeHandler;
using Nexus.Music.Choice.Worker.Services;
using Nexus.Music.Choice.Worker.Services.Integrations;
using Nexus.Music.Choice.Worker.Services.Interfaces;

namespace Nexus.Music.Choice.Worker;

public static class DependecyInjection
{
    public static IServiceCollection AddIPCWorker(this IServiceCollection services)
    {
        return services
            .AddSingleton<IMessageDispatcher<PipeWriter>, PipeEventDispatcher>()
            .AddSingleton<IMessageDispatcher<IStreamWriter>>(sp => sp.GetRequiredService<IMessageDispatcher<PipeWriter>>())
            .AddSingleton<ICommandDispatcher<PipeReader>, PipeCommandDispatcher>()
            .AddSingleton<ICommandDispatcher<IStreamReader>>(sp => sp.GetRequiredService<ICommandDispatcher<PipeReader>>())
            .AddSingleton<IPipeConnectionHandler, PipeConnectionHandler>()
            .AddHostedService<PipeWorker>();
    }

    public static IServiceCollection AddInteractionServices(this IServiceCollection services)
    {
        return services.AddSingleton(new InteractContext())
            .AddSingleton<IInteractionLogService, InteractionLogService>()
            .AddSingleton<IFeedbackService, FeedBackService>()
            .AddSingleton<IInteractionService, InteractionService>()
            .AddSingleton<IVoteService, VoteService>();
    }

    public static IServiceCollection AddIntegrationsBase(this IServiceCollection services)
    {
        return services
            .AddScoped<IHttpProvisioningService, HttpProvisioningService>()
            .AddSingleton<IHttpProvisioningServiceFactory, HttpProvisioningServiceFactory>()
            .AddHostedService<AuthenticationMonitorWorker>();
    }
}
