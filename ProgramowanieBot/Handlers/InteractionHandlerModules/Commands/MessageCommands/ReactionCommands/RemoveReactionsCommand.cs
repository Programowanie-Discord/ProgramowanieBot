using System.Globalization;

using NetCord;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.MessageCommands.ReactionCommands;

public class RemoveReactionsCommand : ApplicationCommandModule<MessageCommandContext>
{
    private readonly ConfigService _config;

    public RemoveReactionsCommand(ConfigService config)
    {
        _config = config;
    }

    [RequireHelpChannel<MessageCommandContext>]
    [RequireOwnMessage<MessageCommandContext>]
    [RequireNotStartMessage<MessageCommandContext>]
    [MessageCommand("Remove Reactions", NameTranslationsProviderType = typeof(NameTranslationsProvider))]
    public async Task RemoveReactionsAsync()
    {
        var message = Context.Target;
        var task = message.DeleteReactionAsync("⬆️");
        await message.DeleteReactionAsync("⬇️");
        await task;
        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Content = $"**{_config.Emojis.Success} {_config.Interaction.ReactionCommands.ReactionsRemovedResponse}**",
            Flags = MessageFlags.Ephemeral,
        }));
    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Usuń reakcje" },
        };
    }
}
