using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.UserMenuInteractions;

public class ResolveInteraction : InteractionModule<ExtendedUserMenuInteractionContext>
{
    [Interaction("resolve")]
    public async Task ResolveAsync()
    {
        var channelId = Context.Interaction.ChannelId.GetValueOrDefault();
        await using (var context = Context.Provider.GetRequiredService<DataContext>())
        {
            if (await context.ResolvedPosts.AnyAsync(p => p.Id == channelId))
                throw new(Context.Config.Interaction.PostAlreadyResolvedResponse);
        }

        var helper = Context.SelectedValues[0];
        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Content = $"**{Context.Config.Emojis.Success} {string.Format(Context.Config.Interaction.WaitingForApprovalResponse, $"<@{helper}>")}**",
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"approve:{helper}:{helper != Context.User.Id}", Context.Config.Interaction.ApproveButtonLabel, ButtonStyle.Success),
                }),
            },
            AllowedMentions = AllowedMentionsProperties.None,
        }));
    }
}
