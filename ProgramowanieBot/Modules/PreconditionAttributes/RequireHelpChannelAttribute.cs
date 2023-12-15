using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Services;

namespace ProgramowanieBot.InteractionHandlerModules;

internal class RequireHelpChannelAttribute<TContext> : PreconditionAttribute<TContext> where TContext : IChannelContext
{
    public override ValueTask EnsureCanExecuteAsync(TContext context, IServiceProvider? serviceProvider)
    {
        var configuration = serviceProvider!.GetRequiredService<Configuration>();
        if (context.Channel is not PublicGuildThread thread || thread.ParentId != configuration.GuildThread.HelpChannelId)
            throw new(configuration.Interaction.NotHelpChannelResponse);

        return default;
    }
}
