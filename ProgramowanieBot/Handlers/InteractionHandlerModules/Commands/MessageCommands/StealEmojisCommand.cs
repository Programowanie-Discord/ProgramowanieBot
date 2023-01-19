using System.Globalization;
using System.Text.RegularExpressions;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.MessageCommands;

public partial class StealEmojisCommand : ApplicationCommandModule<ExtendedMessageCommandContext>
{
    [MessageCommand("Steal Emojis", DefaultGuildUserPermissions = Permissions.ManageEmojisAndStickers, NameTranslationsProviderType = typeof(NameTranslationsProvider))]
    public Task StealEmojisAsync()
    {
        var matches = GetEmojiRegex().Matches(Context.Target.Content);
        if (matches.Count == 0)
            throw new(Context.Config.Interaction.StealEmoji.NoEmojisFoundResponse);

        return RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Components = new ComponentProperties[]
            {
                new StringMenuProperties("steal-emoji", from match in matches
                                                        let stringId = match.Groups[3].Value
                                                        let result = (Success: ulong.TryParse(stringId, NumberStyles.None, CultureInfo.InvariantCulture, out var id), Id: id)
                                                        where result.Success
                                                        let animated = !match.Groups[1].ValueSpan.IsEmpty
                                                        let name = match.Groups[2].Value
                                                        select new StringMenuSelectOptionProperties(name, $"{animated}:{stringId}:{name}")
                                                        {
                                                            Emoji = new(result.Id),
                                                        })
                {
                    Placeholder = Context.Config.Interaction.StealEmoji.StealEmojisMenuPlaceholder,
                },
            },
            Flags = MessageFlags.Ephemeral,
        }));
    }

    [GeneratedRegex(@"<(a?):(\w+):(\d+)>")]
    public static partial Regex GetEmojiRegex();

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Ukradnij emoji" },
        };
    }
}
