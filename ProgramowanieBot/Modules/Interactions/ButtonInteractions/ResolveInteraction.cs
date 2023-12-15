using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.InteractionHandlerModules.Interactions.ButtonInteractions;

public class ResolveInteraction(IServiceProvider serviceProvider, Configuration configuration) : InteractionModule<ButtonInteractionContext>
{
    [Interaction("resolve")]
    public async Task<InteractionCallback> ResolveAsync(ulong helper)
    {
        var channel = Context.Channel;
        var channelId = channel.Id;
        await using (var context = serviceProvider.GetRequiredService<DataContext>())
            if (await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved))
                throw new(configuration.Interaction.PostAlreadyResolvedResponse);

        await Context.Client.Rest.SendMessageAsync(configuration.Interaction.PostResolvedNotificationChannelId, $"**{string.Format(configuration.Interaction.PostResolvedNotificationMessage, channel)}**");

        return InteractionCallback.Message(new()
        {
            Content = $"**{configuration.Emojis.Success} {string.Format(configuration.Interaction.WaitingForApprovalResponse, $"<@{helper}>")}**",
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"approve:{helper}:{helper != Context.User.Id}::", configuration.Interaction.ApproveButtonLabel, ButtonStyle.Success),
                }),
            },
            AllowedMentions = AllowedMentionsProperties.None,
        });
    }
}
