using System.Globalization;

using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.InteractionHandlerModules.Commands.MessageCommands.ReactionCommands;

public class AddReactionsCommand(IOptions<Configuration> options) : ApplicationCommandModule<MessageCommandContext>
{
    [RequireHelpChannel<MessageCommandContext>]
    [RequireOwnMessage<MessageCommandContext>]
    [RequireNotStartMessage<MessageCommandContext>]
    [MessageCommand("Add Reactions", NameTranslationsProviderType = typeof(NameTranslationsProvider))]
    public async Task<InteractionCallback> AddReactionsAsync()
    {
        var message = Context.Target;
        await message.AddReactionAsync("⬆️");
        await message.AddReactionAsync("⬇️");
        var configuration = options.Value;
        return InteractionCallback.Message(new()
        {
            Content = $"**{configuration.Emojis.Success} {configuration.Interaction.ReactionCommands.ReactionsAddedResponse}**",
            Flags = MessageFlags.Ephemeral,
        });
    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Dodaj reakcje" },
        };
    }
}
