using System.Globalization;

using NetCord;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.SlashCommands.ReputationCommands;

public class LeaderboardCommand : ApplicationCommandModule<SlashCommandContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigService _config;

    public LeaderboardCommand(IServiceProvider serviceProvider, ConfigService config)
    {
        _serviceProvider = serviceProvider;
        _config = config;
    }

    [SlashCommand("leaderboard", "Shows reputation leaderboard", NameTranslationsProviderType = typeof(NameTranslationsProvider), DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider))]
    public async Task LeaderboardAsync()
    {
        await RespondAsync(InteractionCallback.ChannelMessageWithSource(await LeaderboardHelper.CreateLeaderboardAsync(Context, _serviceProvider, _config, 0)));
    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "tablica-wyników" },
        };
    }

    public class DescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Pokazuje tablicę wyników w reputacji" },
        };
    }
}
