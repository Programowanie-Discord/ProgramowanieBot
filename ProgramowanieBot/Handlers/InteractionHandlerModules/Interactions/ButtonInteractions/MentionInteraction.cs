using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ButtonInteractions;

public class MentionInteraction : InteractionModule<ExtendedButtonInteractionContext>
{
    [Interaction("mention")]
    public async Task MentionAsync([AllowedUser<ExtendedButtonInteractionContext>] ulong threadOwnerId, ulong roleId)
    {
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
        await ThreadHelper.MentionRoleAsync(Context.Client.Rest, Context.Interaction.ChannelId.GetValueOrDefault(), roleId, Context.Guild!);
    }
}
