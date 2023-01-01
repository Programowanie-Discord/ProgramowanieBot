using NetCord.Services;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;

internal class AllowedUserAttribute<TContext> : ParameterPreconditionAttribute<TContext> where TContext : IUserContext, IExtendedContext
{
    public override ValueTask EnsureCanExecuteAsync(object? value, TContext context)
    {
        if ((ulong)value! != context.User.Id)
            throw new(context.Config.Interaction.OnlyPostCreatorResponse);

        return default;
    }
}
