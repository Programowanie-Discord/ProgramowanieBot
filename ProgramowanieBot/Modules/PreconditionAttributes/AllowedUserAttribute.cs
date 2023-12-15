using Microsoft.Extensions.DependencyInjection;

using NetCord.Services;

namespace ProgramowanieBot.InteractionHandlerModules;

internal class AllowedUserAttribute<TContext> : ParameterPreconditionAttribute<TContext> where TContext : IUserContext
{
    public override ValueTask EnsureCanExecuteAsync(object? value, TContext context, IServiceProvider? serviceProvider)
    {
        if ((ulong)value! != context.User.Id)
            throw new(serviceProvider!.GetRequiredService<Configuration>().Interaction.OnlyPostCreatorResponse);

        return default;
    }
}
