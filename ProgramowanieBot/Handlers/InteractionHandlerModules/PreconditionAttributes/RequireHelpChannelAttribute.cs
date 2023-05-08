using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Services;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules;

internal class RequireHelpChannelAttribute<TContext> : PreconditionAttribute<TContext> where TContext : IChannelContext
{
    public override ValueTask EnsureCanExecuteAsync(TContext context, IServiceProvider? serviceProvider)
    {
        var config = serviceProvider!.GetRequiredService<ConfigService>();
        if (context.Channel is not PublicGuildThread thread || thread.ParentId != config.GuildThread.HelpChannelId)
            throw new(config.Interaction.NotHelpChannelResponse);

        return default;
    }
}
