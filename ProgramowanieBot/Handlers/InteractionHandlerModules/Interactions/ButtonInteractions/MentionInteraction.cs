using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ButtonInteractions;

public class MentionInteraction(ConfigService config) : InteractionModule<ButtonInteractionContext>
{
    private static readonly HashSet<ulong> _mentionedThreadIds = [];

    [Interaction("mention")]
    public async Task MentionAsync([AllowedUser<ButtonInteractionContext>] ulong threadOwnerId, ulong roleId)
    {
        var channelId = Context.Interaction.Channel.Id;

        var mentionedThreadIds = _mentionedThreadIds;
        lock (mentionedThreadIds)
        {
            if (mentionedThreadIds.Contains(channelId))
                throw new(config.Interaction.AlreadyMentionedResponse);
            mentionedThreadIds.Add(channelId);
        }

        await RespondAsync(InteractionCallback.ModifyMessage(m =>
        {
            m.Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"close:{threadOwnerId}", config.GuildThread.PostCloseButtonLabel, ButtonStyle.Danger),
                }),
            };
        }));
        await ThreadHelper.MentionRoleAsync(Context.Client.Rest, channelId, roleId, Context.Guild!);
    }
}
