using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.StringMenuInteractions;

public class StealEmojiInteraction(ConfigService config) : InteractionModule<StringMenuInteractionContext>
{
    [Interaction("steal-emoji")]
    public InteractionCallback StealEmoji()
    {
        var value = Context.SelectedValues[0];

        var index = value.LastIndexOf(':');
        return InteractionCallback.Modal(new($"steal-emoji:{value.AsSpan(0, index)}", config.Interaction.StealEmoji.AddEmojiModalTitle, new TextInputProperties[]
        {
            new("name", TextInputStyle.Short, config.Interaction.StealEmoji.AddEmojiModalNameInputLabel)
            {
                Required = false,
                Value = value[(index + 1)..],
                MaxLength = 32,
            }
        }));
    }
}
