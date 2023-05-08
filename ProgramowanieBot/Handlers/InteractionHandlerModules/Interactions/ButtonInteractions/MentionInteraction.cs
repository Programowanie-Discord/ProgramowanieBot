using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ButtonInteractions;

public class MentionInteraction : InteractionModule<ButtonInteractionContext>
{
    private static readonly HashSet<ulong> _mentionedThreadIds = new();

    private readonly ConfigService _config;

    public MentionInteraction(ConfigService config)
    {
        _config = config;
    }

    [Interaction("mention")]
    public async Task MentionAsync([AllowedUser<ButtonInteractionContext>] ulong threadOwnerId, ulong roleId)
    {
        var channelId = Context.Interaction.Channel.Id;
        lock (_mentionedThreadIds)
        {
            if (_mentionedThreadIds.Contains(channelId))
                throw new(_config.Interaction.AlreadyMentionedResponse);
            _mentionedThreadIds.Add(channelId);
        }

        await RespondAsync(InteractionCallback.UpdateMessage(new()
        {
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"close:{threadOwnerId}", _config.GuildThread.PostCloseButtonLabel, ButtonStyle.Danger),
                }),
            },
        }));
        await ThreadHelper.MentionRoleAsync(Context.Client.Rest, channelId, roleId, Context.Guild!);
    }
}
