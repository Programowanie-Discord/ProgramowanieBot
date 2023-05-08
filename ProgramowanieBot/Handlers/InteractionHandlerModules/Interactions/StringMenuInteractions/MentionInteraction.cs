using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.StringMenuInteractions;

public class MentionInteraction : InteractionModule<StringMenuInteractionContext>
{
    private readonly ConfigService _config;

    public MentionInteraction(ConfigService config)
    {
        _config = config;
    }

    [Interaction("mention")]
    public async Task MentionAsync([AllowedUser<StringMenuInteractionContext>] ulong threadOwnerId)
    {
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
        await ThreadHelper.MentionRoleAsync(Context.Client.Rest, Context.Interaction.Channel.Id, ulong.Parse(Context.SelectedValues[0]), Context.Guild!);
    }
}
