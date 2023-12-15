using System.Globalization;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.InteractionHandlerModules.Commands.MessageCommands.ReactionCommands;

public class AddReactionsCommand(Configuration configuration) : ApplicationCommandModule<MessageCommandContext>
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
