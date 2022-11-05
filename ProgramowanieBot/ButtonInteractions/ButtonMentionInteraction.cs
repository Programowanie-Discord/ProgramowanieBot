using System.Threading;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.CustomContexts;

namespace ProgramowanieBot.ButtonInteractions;

public class ButtonMentionInteraction : InteractionModule<ButtonInteractionContextWithConfig>
{
    [Interaction("mention")]
    public async Task MentionAsync([AllowedUser<ButtonInteractionContextWithConfig>] ulong threadOwnerId, ulong roleId)
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
        await ThreadHelper.MentionRoleAsync(Context.Client.Rest, Context.Channel.Id, roleId, Context.Guild!);
    }
}
