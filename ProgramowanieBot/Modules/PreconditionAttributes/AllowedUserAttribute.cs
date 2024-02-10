using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord.Services;

namespace ProgramowanieBot.InteractionHandlerModules;

internal class AllowedUserAttribute<TContext> : ParameterPreconditionAttribute<TContext> where TContext : IUserContext
{
    public override ValueTask<PreconditionResult> EnsureCanExecuteAsync(object? value, TContext context, IServiceProvider? serviceProvider)
    {
        if ((ulong)value! != context.User.Id)
            return new(PreconditionResult.Fail(serviceProvider!.GetRequiredService<IOptions<Configuration>>().Value.Interaction.OnlyPostCreatorResponse));

        return new(PreconditionResult.Success);
    }
}
