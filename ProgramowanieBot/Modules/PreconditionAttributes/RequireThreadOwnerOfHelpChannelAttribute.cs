using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Services;

namespace ProgramowanieBot.InteractionHandlerModules;

internal class RequireThreadOwnerOfHelpChannelAttribute<TContext> : PreconditionAttribute<TContext> where TContext : IUserContext, IChannelContext
{
    public override ValueTask<PreconditionResult> EnsureCanExecuteAsync(TContext context, IServiceProvider? serviceProvider)
    {
        var configuration = serviceProvider!.GetRequiredService<IOptions<Configuration>>().Value;
        if (context.Channel is not PublicGuildThread thread || thread.ParentId != configuration.GuildThread.HelpChannelId || thread.OwnerId != context.User.Id)
            return new(PreconditionResult.Fail(configuration.Interaction.NotHelpChannelResponse));

        return new(PreconditionResult.Success);
    }
}
