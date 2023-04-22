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
            if (await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved))
                throw new(Context.Config.Interaction.PostAlreadyResolvedResponse);
        }

        var values = Context.SelectedUsers;

        var helper = values[0];
        if (helper.IsBot)
            throw new(Context.Config.Interaction.SelectedBotAsHelperResponse);

        var isHelper2 = values.Count == 2;
        User? helper2;
        if (isHelper2)
        {
            helper2 = values[1];
            if (helper2.IsBot)
                throw new(Context.Config.Interaction.SelectedBotAsHelperResponse);
        }
        else
            helper2 = null;

        var user = Context.User;
        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Content = $"**{Context.Config.Emojis.Success} {(isHelper2 ? string.Format(Context.Config.Interaction.WaitingForApprovalWith2HelpersResponse, helper, helper2) : string.Format(Context.Config.Interaction.WaitingForApprovalResponse, helper))}**",
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"approve:{helper.Id}:{helper != user}:{(isHelper2 ? helper2!.Id : null)}:{(isHelper2 ? helper2 != user : null)}", Context.Config.Interaction.ApproveButtonLabel, ButtonStyle.Success),
                }),
            },
            AllowedMentions = AllowedMentionsProperties.None,
        }));
    }
}
