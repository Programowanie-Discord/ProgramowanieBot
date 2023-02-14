using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ButtonInteractions;

public class MentionInteraction : InteractionModule<ExtendedButtonInteractionContext>
{
    private static readonly HashSet<ulong> _mentionedThreadIds = new();

    [Interaction("mention")]
    public async Task MentionAsync([AllowedUser<ExtendedButtonInteractionContext>] ulong threadOwnerId, ulong roleId)
    {
        var channelId = Context.Interaction.ChannelId.GetValueOrDefault();
        lock (_mentionedThreadIds)
        {
            if (_mentionedThreadIds.Contains(channelId))
                throw new(Context.Config.Interaction.AlreadyMentionedResponse);
            _mentionedThreadIds.Add(channelId);
        }

        await RespondAsync(InteractionCallback.UpdateMessage(new()
        {
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"close:{threadOwnerId}", Context.Config.GuildThread.PostCloseButtonLabel, ButtonStyle.Danger),
                }),
            },
        }));
        await ThreadHelper.MentionRoleAsync(Context.Client.Rest, channelId, roleId, Context.Guild!);
    }
}
