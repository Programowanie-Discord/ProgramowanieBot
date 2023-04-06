using System.Globalization;
using System.Text.RegularExpressions;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.MessageCommands;

public partial class StealEmojisCommand : ApplicationCommandModule<ExtendedMessageCommandContext>
{
    [MessageCommand("Steal Emojis", DefaultGuildUserPermissions = Permissions.ManageGuildExpressions, NameTranslationsProviderType = typeof(NameTranslationsProvider))]
    public Task StealEmojisAsync()
    {
        var matches = GetEmojiRegex().Matches(Context.Target.Content);
        if (matches.Count == 0)
            throw new(Context.Config.Interaction.StealEmoji.NoEmojisFoundResponse);

        return RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Components = new ComponentProperties[]
            {
                new StringMenuProperties("steal-emoji", GetOptions())
                {
                    Placeholder = Context.Config.Interaction.StealEmoji.StealEmojisMenuPlaceholder,
                },
            },
            Flags = MessageFlags.Ephemeral,
        }));

        IEnumerable<StringMenuSelectOptionProperties> GetOptions()
        {
            HashSet<ulong> ids = new(25);
            foreach (var match in matches.Take(25))
            {
                var stringId = match.Groups[3].Value;
                if (!ulong.TryParse(stringId, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || ids.Contains(id))
                    continue;
                ids.Add(id);

                var animated = !match.Groups[1].ValueSpan.IsEmpty;
                var name = match.Groups[2].Value;
                yield return new(name, $"{animated}:{stringId}:{name}")
                {
                    Emoji = new(id),
                };
            }
        }
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
