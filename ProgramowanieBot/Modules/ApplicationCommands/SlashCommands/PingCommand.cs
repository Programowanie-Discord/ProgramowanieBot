using System.Globalization;

using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.InteractionHandlerModules.Commands.SlashCommands;

public class PingCommand : ApplicationCommandModule<SlashCommandContext>
{
    [SlashCommand("ping", "Shows bot's latency", DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider), DMPermission = true)]
    public InteractionCallback Ping()
    {
        return InteractionCallback.Message($"**Pong! {Math.Round(Context.Client.Latency.TotalMilliseconds)} ms**");
    }

    public class DescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Pokazuje opóźnienie bota" },
        };
    }
}
