using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.StringMenuInteractions;

public class StealEmojiInteraction : InteractionModule<StringMenuInteractionContext>
{
    private readonly ConfigService _config;

    public StealEmojiInteraction(ConfigService config)
    {
        _config = config;
    }

    [Interaction("steal-emoji")]
    public Task StealEmojiAsync()
    {
        var value = Context.SelectedValues[0];

        var index = value.LastIndexOf(':');
        return RespondAsync(InteractionCallback.Modal(new($"steal-emoji:{value.AsSpan(0, index)}", _config.Interaction.StealEmoji.AddEmojiModalTitle, new TextInputProperties[]
        {
            new("name", TextInputStyle.Short, _config.Interaction.StealEmoji.AddEmojiModalNameInputLabel)
            {
                Required = false,
                Value = value[(index + 1)..],
                MaxLength = 32,
            }
        })));
    }
}
