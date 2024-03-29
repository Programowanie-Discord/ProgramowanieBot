﻿using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.InteractionHandlerModules.Interactions.ModalSubmitInteractions;

public class StealEmojiInteraction(IHttpClientFactory httpClientFactory) : InteractionModule<ModalSubmitInteractionContext>
{
    [Interaction("steal-emoji")]
    public async Task<InteractionCallback> StealEmojiAsync(bool animated, ulong id)
    {
        var format = animated ? ImageFormat.Gif : ImageFormat.Png;

        byte[] data;
        using (var httpClient = httpClientFactory.CreateClient())
            data = await httpClient.GetByteArrayAsync(ImageUrl.CustomEmoji(id, format).ToString());

        var emoji = await Context.Client.Rest.CreateGuildEmojiAsync(Context.Interaction.GuildId.GetValueOrDefault(), new(Context.Components[0].Value.Trim().Replace(' ', '_').PadRight(2, '_'), new(format, data)));

        return InteractionCallback.Message(new()
        {
            Content = $"**Emoji {emoji} successfully created!**",
            Flags = MessageFlags.Ephemeral,
        });
    }
}
