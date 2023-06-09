using NetCord;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ModalSubmitInteractions;

public class StealEmojiInteraction : InteractionModule<ModalSubmitInteractionContext>
{
    private readonly HttpClient _httpClient;

    public StealEmojiInteraction(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [Interaction("steal-emoji")]
    public async Task StealEmojiAsync(bool animated, ulong id)
    {
        var format = animated ? ImageFormat.Gif : ImageFormat.Png;

        var data = await _httpClient.GetByteArrayAsync(ImageUrl.CustomEmoji(id, format).ToString());

        var emoji = await Context.Client.Rest.CreateGuildEmojiAsync(Context.Interaction.GuildId.GetValueOrDefault(), new(Context.Components[0].Value.Trim().Replace(' ', '_').PadRight(2, '_'), new(format, data)));

        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Content = $"**Emoji {emoji} successfully created!**",
            Flags = MessageFlags.Ephemeral,
        }));
    }
}
