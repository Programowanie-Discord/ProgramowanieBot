using NetCord;
using NetCord.Services;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules;

internal class RequireHelpChannel<TContext> : PreconditionAttribute<TContext> where TContext : IChannelContext, IExtendedContext
{
    public override ValueTask EnsureCanExecuteAsync(TContext context)
    {
        if (context.Channel is not PublicGuildThread thread || thread.ParentId != context.Config.GuildThread.HelpChannelId)
            throw new(context.Config.Interaction.NotHelpChannelResponse);

        return default;
    }
}
