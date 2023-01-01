using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.ButtonInteractions;

public class PostCloseInteraction : InteractionModule<ExtendedButtonInteractionContext>
{
    [Interaction("close")]
    public async Task CloseAsync([AllowedUserOrModerator<ExtendedButtonInteractionContext>] ulong threadOwnerId)
    {
        try
        {
            await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
            {
                Content = $"**{Context.Config.Emojis.Success} {Context.Config.Interaction.PostClosedResponse}**",
                Flags = MessageFlags.Ephemeral,
            }));
        }
        catch (RestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            if (await ex.GetDiscordStatusCodeAsync() == 50083)
                return;
            else
                throw;
        }
        await Context.Client.Rest.ModifyGuildThreadAsync(Context.Interaction.ChannelId.GetValueOrDefault(), c => c.Archived = true);
    }
}
