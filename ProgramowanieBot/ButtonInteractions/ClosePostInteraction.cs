using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.CustomContexts;

namespace ProgramowanieBot.ButtonInteractions;

public class ClosePostInteraction : InteractionModule<ButtonInteractionContextWithConfig>
{
    [Interaction("close")]
    public async Task CloseAsync([AllowedUserOrModerator<ButtonInteractionContextWithConfig>] ulong threadOwnerId)
    {
        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Content = Context.Config.PostCloseResponse,
            Flags = MessageFlags.Ephemeral,
        }));
        await Context.Client.Rest.ModifyGuildThreadAsync(Context.Interaction.ChannelId.GetValueOrDefault(), c => c.Archived = true);
    }
}
