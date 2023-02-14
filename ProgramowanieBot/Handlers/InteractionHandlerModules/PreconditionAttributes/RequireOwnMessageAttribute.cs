using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules;

internal class RequireOwnMessageAttribute<TContext> : PreconditionAttribute<TContext> where TContext : MessageCommandContext, IExtendedContext
{
    public override ValueTask EnsureCanExecuteAsync(TContext context)
    {
        if (context.Target.Author != context.User)
            throw new(context.Config.Interaction.NotOwnMessageResponse);

        return default;
    }
}
