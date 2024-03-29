﻿using System.Globalization;

using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.InteractionHandlerModules.Commands.MessageCommands.ReactionCommands;

public class RemoveReactionsCommand(IOptions<Configuration> options) : ApplicationCommandModule<MessageCommandContext>
{
    [RequireHelpChannel<MessageCommandContext>]
    [RequireOwnMessage<MessageCommandContext>]
    [RequireNotStartMessage<MessageCommandContext>]
    [MessageCommand("Remove Reactions", NameTranslationsProviderType = typeof(NameTranslationsProvider))]
    public async Task<InteractionCallback> RemoveReactionsAsync()
    {
        var message = Context.Target;
        var task = message.DeleteReactionAsync("⬆️");
        await message.DeleteReactionAsync("⬇️");
        await task;
        var configuration = options.Value;
        return InteractionCallback.Message(new()
        {
            Content = $"**{configuration.Emojis.Success} {configuration.Interaction.ReactionCommands.ReactionsRemovedResponse}**",
            Flags = MessageFlags.Ephemeral,
        });
    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Usuń reakcje" },
        };
    }
}
