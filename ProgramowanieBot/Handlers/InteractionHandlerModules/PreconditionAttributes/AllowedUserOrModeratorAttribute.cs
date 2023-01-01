using NetCord;
using NetCord.Gateway;
using NetCord.Services;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;

internal class AllowedUserOrModeratorAttribute<TContext> : ParameterPreconditionAttribute<TContext> where TContext : IUserContext, IExtendedContext
{
    public override ValueTask EnsureCanExecuteAsync(object? value, TContext context)
    {
        if ((ulong)value! != context.User.Id && !((GuildInteractionUser)context.User).Permissions.HasFlag(Permissions.ManageThreads))
            throw new(context.Config.Interaction.OnlyPostCreatorOrModeratorResponse);

        return default;
    }
}
