using NetCord.Gateway;
using NetCord.Services;

using ProgramowanieBot.CustomContexts;

namespace ProgramowanieBot;

internal class AllowedUserOrModeratorAttribute<TContext> : ParameterPreconditionAttribute<TContext> where TContext : IUserContext, IInteractionContextWithConfig
{
    public override ValueTask EnsureCanExecuteAsync(object? value, TContext context)
    {
        if ((ulong)value! != context.User.Id && !((GuildInteractionUser)context.User).Permissions.HasFlag(NetCord.Permission.ManageThreads))
            throw new(context.Config.OnlyPostCreatorOrModeratorResponse);

        return default;
    }
}
