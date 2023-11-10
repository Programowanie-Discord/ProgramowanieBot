using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ButtonInteractions;

public class ResolveInteraction(IServiceProvider serviceProvider, ConfigService config) : InteractionModule<ButtonInteractionContext>
{
    [Interaction("resolve")]
    public async Task<InteractionCallback> ResolveAsync(ulong helper)
    {
        var channelId = Context.Interaction.Channel.Id;
        await using (var context = serviceProvider.GetRequiredService<DataContext>())
        {
            if (await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved))
                throw new(config.Interaction.PostAlreadyResolvedResponse);
        }

        return InteractionCallback.Message(new()
        {
            Content = $"**{config.Emojis.Success} {string.Format(config.Interaction.WaitingForApprovalResponse, $"<@{helper}>")}**",
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"approve:{helper}:{helper != Context.User.Id}::", config.Interaction.ApproveButtonLabel, ButtonStyle.Success),
                }),
            },
            AllowedMentions = AllowedMentionsProperties.None,
        });
    }
}
