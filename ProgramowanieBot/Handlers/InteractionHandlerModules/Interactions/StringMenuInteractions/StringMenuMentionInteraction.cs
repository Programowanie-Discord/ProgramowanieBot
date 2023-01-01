using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.StringMenuInteractions;

public class StringMenuMentionInteraction : InteractionModule<ExtendedStringMenuInteractionContext>
{
    [Interaction("mention")]
    public async Task MentionAsync([AllowedUser<ExtendedStringMenuInteractionContext>] ulong threadOwnerId)
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
        await ThreadHelper.MentionRoleAsync(Context.Client.Rest, Context.Interaction.ChannelId.GetValueOrDefault(), ulong.Parse(Context.SelectedValues[0]), Context.Guild!);
    }
}
