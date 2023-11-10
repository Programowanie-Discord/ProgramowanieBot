using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Services;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;

internal class AllowedUserOrModeratorAttribute<TContext> : ParameterPreconditionAttribute<TContext> where TContext : IUserContext
{
    public override ValueTask EnsureCanExecuteAsync(object? value, TContext context, IServiceProvider? serviceProvider)
    {
        if ((ulong)value! != context.User.Id && !((GuildInteractionUser)context.User).Permissions.HasFlag(Permissions.ManageThreads))
            throw new(serviceProvider!.GetRequiredService<ConfigService>().Interaction.OnlyPostCreatorOrModeratorResponse);

        return default;
    }
}
