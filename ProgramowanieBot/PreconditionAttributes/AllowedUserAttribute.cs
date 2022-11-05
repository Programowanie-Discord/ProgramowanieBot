using NetCord.Services;

using ProgramowanieBot.CustomContexts;

namespace ProgramowanieBot;

internal class AllowedUserAttribute<TContext> : ParameterPreconditionAttribute<TContext> where TContext : IUserContext, IInteractionContextWithConfig
{
    public override ValueTask EnsureCanExecuteAsync(object? value, TContext context)
    {
        if ((ulong)value! != context.User.Id)
            throw new(context.Config.OnlyPostCreatorResponse);

        return default;
    }
}
