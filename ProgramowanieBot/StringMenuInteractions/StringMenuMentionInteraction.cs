using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.CustomContexts;

namespace ProgramowanieBot.StringMenuInteractions;

public class StringMenuMentionInteraction : InteractionModule<StringMenuInteractionContextWithConfig>
{
    [Interaction("mention")]
    public async Task MentionAsync(ulong allowedUserId)
    {
        if (allowedUserId != Context.User.Id)
            throw new(Context.Config.OnlyPostCreatorResponse);

        await RespondAsync(InteractionCallback.UpdateMessage(new()
        {
            Components = Enumerable.Empty<ComponentProperties>(),
        }));
        await ThreadHelper.MentionRoleAsync(Context.Client.Rest, Context.Channel.Id, ulong.Parse(Context.SelectedValues[0]), Context.Guild!);
    }
}
