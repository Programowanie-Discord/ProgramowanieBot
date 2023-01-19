using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.StringMenuInteractions;

public class StealEmojiInteraction : InteractionModule<ExtendedStringMenuInteractionContext>
{
    [Interaction("steal-emoji")]
    public Task StealEmojiAsync()
    {
        var value = Context.SelectedValues[0];

        var index = value.LastIndexOf(':');
        return RespondAsync(InteractionCallback.Modal(new($"steal-emoji:{value.AsSpan(0, index)}", Context.Config.Interaction.StealEmoji.AddEmojiModalTitle, new TextInputProperties[]
        {
            new("name", TextInputStyle.Short, Context.Config.Interaction.StealEmoji.AddEmojiModalNameInputLabel)
            {
                Required = false,
                Value = value[(index + 1)..],
                MaxLength = 32,
            }
        })));
    }
}
