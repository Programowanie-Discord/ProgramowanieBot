using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.CustomContexts;

namespace ProgramowanieBot.StringMenuInteractions;

public class StringMenuMentionInteraction : InteractionModule<StringMenuInteractionContextWithConfig>
{
    [Interaction("mention")]
    public async Task MentionAsync([AllowedUser<StringMenuInteractionContextWithConfig>] ulong threadOwnerId)
    {
        await RespondAsync(InteractionCallback.UpdateMessage(new()
        {
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"close:{threadOwnerId}", "Zamknij", ButtonStyle.Danger),
                }),
            },
        }));
        await ThreadHelper.MentionRoleAsync(Context.Client.Rest, Context.Interaction.ChannelId.GetValueOrDefault(), ulong.Parse(Context.SelectedValues[0]), Context.Guild!);
    }
}
