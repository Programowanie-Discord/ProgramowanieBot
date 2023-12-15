using Microsoft.Extensions.DependencyInjection;

using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.InteractionHandlerModules;

internal class RequireOwnMessageAttribute<TContext> : PreconditionAttribute<TContext> where TContext : MessageCommandContext
{
    public override ValueTask EnsureCanExecuteAsync(TContext context, IServiceProvider? serviceProvider)
    {
        if (context.Target.Author != context.User)
            throw new(serviceProvider!.GetRequiredService<Configuration>().Interaction.NotOwnMessageResponse);

        return default;
    }
}
