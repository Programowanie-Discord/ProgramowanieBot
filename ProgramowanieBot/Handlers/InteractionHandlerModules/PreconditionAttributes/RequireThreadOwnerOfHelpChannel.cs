using NetCord;
using NetCord.Services;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;

internal class RequireThreadOwnerOfHelpChannel<TContext> : PreconditionAttribute<TContext> where TContext : IUserContext, IChannelContext, IExtendedContext
{
    public override ValueTask EnsureCanExecuteAsync(TContext context)
    {
        if (context.Channel is not PublicGuildThread thread || thread.ParentId != context.Config.GuildThread.HelpChannelId || thread.OwnerId != context.User.Id)
            throw new(context.Config.Interaction.NotHelpChannelResponse);

        return default;
    }
}
