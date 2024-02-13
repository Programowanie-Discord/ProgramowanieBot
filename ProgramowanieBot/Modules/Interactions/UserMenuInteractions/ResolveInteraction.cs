using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.InteractionHandlerModules.Interactions.UserMenuInteractions;

public class ResolveInteraction(IServiceProvider serviceProvider, IOptions<Configuration> options) : InteractionModule<UserMenuInteractionContext>
{
    [Interaction("resolve")]
    public async Task<InteractionCallback> ResolveAsync()
    {
        var configuration = options.Value;
        var channelId = Context.Channel.Id;

        await using (var context = serviceProvider.GetRequiredService<DataContext>())
            if (await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved))
                throw new(configuration.Interaction.PostAlreadyResolvedResponse);

        var values = Context.SelectedUsers;

        var helper = values[0];
        if (helper.IsBot)
            throw new(configuration.Interaction.SelectedBotAsHelperResponse);

        var isHelper2 = values.Count == 2;
        User? helper2;
        if (isHelper2)
        {
            helper2 = values[1];
            if (helper2.IsBot)
                throw new(configuration.Interaction.SelectedBotAsHelperResponse);
        }
        else
            helper2 = null;

        await PostsHelper.SendPostResolveMessages(channelId, Context.User.Id, helper.Id, helper2?.Id, Context.Client.Rest, configuration);

        return InteractionCallback.DeferredModifyMessage;
    }
}
