using Microsoft.Extensions.DependencyInjection;

using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.InteractionHandlerModules;

internal class RequireNotStartMessageAttribute<TContext> : PreconditionAttribute<TContext> where TContext : MessageCommandContext
{
    public override ValueTask EnsureCanExecuteAsync(TContext context, IServiceProvider? serviceProvider)
    {
        if (context.Target.Id == context.Target.ChannelId)
            throw new(serviceProvider!.GetRequiredService<Configuration>().Interaction.ReactionCommands.HelpPostStartMessageResponse);

        return default;
    }
}
