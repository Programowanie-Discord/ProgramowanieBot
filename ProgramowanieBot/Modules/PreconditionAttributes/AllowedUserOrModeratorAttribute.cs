using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Services;

namespace ProgramowanieBot.InteractionHandlerModules;

internal class AllowedUserOrModeratorAttribute<TContext> : ParameterPreconditionAttribute<TContext> where TContext : IUserContext
{
    public override ValueTask<PreconditionResult> EnsureCanExecuteAsync(object? value, TContext context, IServiceProvider? serviceProvider)
    {
        if ((ulong)value! != context.User.Id && !((GuildInteractionUser)context.User).Permissions.HasFlag(Permissions.ManageThreads))
            return new(PreconditionResult.Fail(serviceProvider!.GetRequiredService<IOptions<Configuration>>().Value.Interaction.OnlyPostCreatorOrModeratorResponse));

        return new(PreconditionResult.Success);
    }
}
