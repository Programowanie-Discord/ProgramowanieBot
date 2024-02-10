using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.InteractionHandlerModules;

internal class RequireOwnMessageAttribute<TContext> : PreconditionAttribute<TContext> where TContext : MessageCommandContext
{
    public override ValueTask<PreconditionResult> EnsureCanExecuteAsync(TContext context, IServiceProvider? serviceProvider)
    {
        if (context.Target.Author != context.User)
            return new(PreconditionResult.Fail(serviceProvider!.GetRequiredService<IOptions<Configuration>>().Value.Interaction.NotOwnMessageResponse));

        return new(PreconditionResult.Success);
    }
}
