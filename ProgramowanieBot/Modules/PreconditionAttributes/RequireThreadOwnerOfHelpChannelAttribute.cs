using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Services;

namespace ProgramowanieBot.InteractionHandlerModules;

internal class RequireThreadOwnerOfHelpChannelAttribute<TContext> : PreconditionAttribute<TContext> where TContext : IUserContext, IChannelContext
{
    public override ValueTask EnsureCanExecuteAsync(TContext context, IServiceProvider? serviceProvider)
    {
        var configuration = serviceProvider!.GetRequiredService<Configuration>();
        if (context.Channel is not PublicGuildThread thread || thread.ParentId != configuration.GuildThread.HelpChannelId || thread.OwnerId != context.User.Id)
            throw new(configuration.Interaction.NotHelpChannelResponse);

        return default;
    }
}
