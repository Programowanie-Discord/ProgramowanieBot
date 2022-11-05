using NetCord;
using NetCord.Services.Interactions;

using ProgramowanieBot.CustomContexts;

namespace ProgramowanieBot.ButtonInteractions;

public class ClosePostInteraction : InteractionModule<ButtonInteractionContextWithConfig>
{
    [Interaction("close")]
    public async Task CloseAsync([AllowedUserOrModerator<ButtonInteractionContextWithConfig>] ulong threadOwnerId)
    {
        await ((PublicGuildThread)Context.Channel).ModifyAsync(c => c.Archived = true);
        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Content = Context.Config.PostCloseResponse,
        }));
    }
}
