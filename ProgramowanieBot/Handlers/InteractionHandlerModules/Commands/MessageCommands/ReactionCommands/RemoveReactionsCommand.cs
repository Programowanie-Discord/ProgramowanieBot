using System.Globalization;

using NetCord;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.MessageCommands.ReactionCommands;

public class RemoveReactionsCommand : ApplicationCommandModule<ExtendedMessageCommandContext>
{
    [RequireHelpChannel<ExtendedMessageCommandContext>]
    [RequireOwnMessage<ExtendedMessageCommandContext>]
    [MessageCommand("Remove Reactions", NameTranslationsProviderType = typeof(NameTranslationsProvider))]
    public async Task RemoveReactionsAsync()
    {
        var message = Context.Target;
        var botId = Context.Client.User!.Id;
        var task = message.DeleteReactionAsync("⬆️", botId);
        await message.DeleteReactionAsync("⬇️", botId);
        await task;
        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Content = $"**{Context.Config.Emojis.Success} {Context.Config.Interaction.ReactionCommands.ReactionsRemovedResponse}**",
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
