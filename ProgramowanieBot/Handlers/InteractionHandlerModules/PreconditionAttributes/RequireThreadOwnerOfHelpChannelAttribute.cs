using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Services;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;

internal class RequireThreadOwnerOfHelpChannelAttribute<TContext> : PreconditionAttribute<TContext> where TContext : IUserContext, IChannelContext
{
    public override ValueTask EnsureCanExecuteAsync(TContext context, IServiceProvider? serviceProvider)
    {
        var config = serviceProvider!.GetRequiredService<ConfigService>();
        if (context.Channel is not PublicGuildThread thread || thread.ParentId != config.GuildThread.HelpChannelId || thread.OwnerId != context.User.Id)
            throw new(config.Interaction.NotHelpChannelResponse);

        return default;
    }
}
