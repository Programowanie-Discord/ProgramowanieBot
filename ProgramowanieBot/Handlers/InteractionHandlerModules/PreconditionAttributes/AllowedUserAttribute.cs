using Microsoft.Extensions.DependencyInjection;

using NetCord.Services;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;

internal class AllowedUserAttribute<TContext> : ParameterPreconditionAttribute<TContext> where TContext : IUserContext
{
    public override ValueTask EnsureCanExecuteAsync(object? value, TContext context, IServiceProvider? serviceProvider)
    {
        if ((ulong)value! != context.User.Id)
            throw new(serviceProvider!.GetRequiredService<ConfigService>().Interaction.OnlyPostCreatorResponse);

        return default;
    }
}
