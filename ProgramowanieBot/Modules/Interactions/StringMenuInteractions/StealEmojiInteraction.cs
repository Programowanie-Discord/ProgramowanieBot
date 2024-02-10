using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.InteractionHandlerModules.Interactions.StringMenuInteractions;

public class StealEmojiInteraction(IOptions<Configuration> options) : InteractionModule<StringMenuInteractionContext>
{
    [Interaction("steal-emoji")]
    public InteractionCallback StealEmoji()
    {
        var configuration = options.Value;

        var value = Context.SelectedValues[0];

        var index = value.LastIndexOf(':');
        return InteractionCallback.Modal(new($"steal-emoji:{value.AsSpan(0, index)}", configuration.Interaction.StealEmoji.AddEmojiModalTitle,
        [
            new("name", TextInputStyle.Short, configuration.Interaction.StealEmoji.AddEmojiModalNameInputLabel)
            {
                Required = false,
                Value = value[(index + 1)..],
                MaxLength = 32,
            }
        ]));
    }
}
