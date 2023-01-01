using System.Globalization;

using NetCord;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands;

public class PingCommand : ApplicationCommandModule<ExtendedSlashCommandContext>
{
    [SlashCommand("ping", "Shows bot's latency", DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider), DMPermission = true)]
    public Task PingAsync()
    {
        return RespondAsync(InteractionCallback.ChannelMessageWithSource($"**Pong! {Math.Round(Context.Client.Latency.GetValueOrDefault().TotalMilliseconds)} ms**"));
    }

    public class DescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Pokazuje opóźnienie bota" },
        };
    }
}
