using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ModalSubmitInteractions;

public class StealEmojiInteraction : InteractionModule<ExtendedModalSubmitInteractionContext>
{
    [Interaction("steal-emoji")]
    public async Task StealEmojiAsync(bool animated, ulong id)
    {
        var format = animated ? ImageFormat.Gif : ImageFormat.Png;

        var data = await Context.Provider.GetRequiredService<HttpClient>().GetByteArrayAsync(ImageUrl.CustomEmoji(id, format).ToString());

        var emoji = await Context.Client.Rest.CreateGuildEmojiAsync(Context.Interaction.GuildId.GetValueOrDefault(), new(Context.Components[0].Value.AsSpan().TrimStart().TrimEnd().ToString().Replace(' ', '_').PadRight(2, '_'), new(data, format)));

        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Content = $"**Emoji {emoji} successfully created!**",
            Flags = MessageFlags.Ephemeral,
        }));
    }
}
