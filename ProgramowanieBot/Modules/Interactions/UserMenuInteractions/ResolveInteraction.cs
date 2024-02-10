using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.InteractionHandlerModules.Interactions.UserMenuInteractions;

public class ResolveInteraction(IServiceProvider serviceProvider, IOptions<Configuration> options) : InteractionModule<UserMenuInteractionContext>
{
    [Interaction("resolve")]
    public async Task<InteractionCallback> ResolveAsync()
    {
        var configuration = options.Value;

        var channel = Context.Channel;
        var channelId = channel.Id;
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

        await Context.Client.Rest.SendMessageAsync(configuration.Interaction.PostResolvedNotificationChannelId, $"**{string.Format(configuration.Interaction.PostResolvedNotificationMessage, channel)}**");

        var user = Context.User;
        return InteractionCallback.Message(new()
        {
            Content = $"**{configuration.Emojis.Success} {(isHelper2 ? string.Format(configuration.Interaction.WaitingForApprovalWith2HelpersResponse, helper, helper2) : string.Format(configuration.Interaction.WaitingForApprovalResponse, helper))}**",
            Components =
            [
                new ActionRowProperties(
                [
                    new ActionButtonProperties($"approve:{helper.Id}:{helper != user}:{(isHelper2 ? helper2!.Id : null)}:{(isHelper2 ? helper2 != user : null)}", configuration.Interaction.ApproveButtonLabel, ButtonStyle.Success),
                ]),
            ],
            AllowedMentions = AllowedMentionsProperties.None,
        });
    }
}
