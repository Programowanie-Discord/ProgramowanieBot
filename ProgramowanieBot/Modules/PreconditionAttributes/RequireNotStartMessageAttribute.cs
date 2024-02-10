using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.InteractionHandlerModules;

internal class RequireNotStartMessageAttribute<TContext> : PreconditionAttribute<TContext> where TContext : MessageCommandContext
{
    public override ValueTask<PreconditionResult> EnsureCanExecuteAsync(TContext context, IServiceProvider? serviceProvider)
    {
        if (context.Target.Id == context.Target.ChannelId)
            return new(PreconditionResult.Fail(serviceProvider!.GetRequiredService<IOptions<Configuration>>().Value.Interaction.ReactionCommands.HelpPostStartMessageResponse));

        return new(PreconditionResult.Success);
    }
}
