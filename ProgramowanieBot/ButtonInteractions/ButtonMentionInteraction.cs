using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.CustomContexts;

namespace ProgramowanieBot.ButtonInteractions;

public class ButtonMentionInteraction : InteractionModule<ButtonInteractionContextWithConfig>
{
    [Interaction("mention")]
    public async Task MentionAsync(ulong allowedUserId, ulong roleId)
    {
        if (allowedUserId != Context.User.Id)
            throw new(Context.Config.OnlyPostCreatorResponse);

        await RespondAsync(InteractionCallback.UpdateMessage(new()
        {
            Components = Enumerable.Empty<ComponentProperties>(),
        }));
        await ThreadHelper.MentionRoleAsync(Context.Client.Rest, Context.Channel.Id, roleId, Context.Guild!);
    }
}
