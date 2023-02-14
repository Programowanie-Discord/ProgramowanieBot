using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules;

internal class RequireNotStartMessageAttribute<TContext> : PreconditionAttribute<TContext> where TContext : MessageCommandContext, IExtendedContext
{
    public override ValueTask EnsureCanExecuteAsync(TContext context)
    {
        if (context.Target.Id == context.Target.ChannelId)
            throw new(context.Config.Interaction.ReactionCommands.HelpPostStartMessageResponse);

        return default;
    }
}
