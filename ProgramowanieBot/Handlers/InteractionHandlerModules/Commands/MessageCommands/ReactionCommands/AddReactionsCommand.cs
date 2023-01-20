using System.Globalization;

using NetCord;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.MessageCommands.ReactionCommands;

public class AddReactionsCommand : ApplicationCommandModule<ExtendedMessageCommandContext>
{
    [RequireHelpChannel<ExtendedMessageCommandContext>]
    [RequireOwnMessage<ExtendedMessageCommandContext>]
    [MessageCommand("Add Reactions", NameTranslationsProviderType = typeof(NameTranslationsProvider))]
    public async Task AddReactionsAsync()
    {
        var message = Context.Target;
        await message.AddReactionAsync("⬆️");
        await message.AddReactionAsync("⬇️");
        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Content = $"**{Context.Config.Emojis.Success} {Context.Config.Interaction.ReactionCommands.ReactionsAddedResponse}**",
            Flags = MessageFlags.Ephemeral,
        }));
    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Dodaj reakcje" },
        };
    }
}
