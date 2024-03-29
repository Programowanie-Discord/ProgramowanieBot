﻿using System.Globalization;

using Microsoft.Extensions.Options;

using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.InteractionHandlerModules.Commands.SlashCommands.ReputationCommands;

public class LeaderboardCommand(IServiceProvider serviceProvider, IOptions<Configuration> options) : ApplicationCommandModule<SlashCommandContext>
{
    [SlashCommand("leaderboard", "Shows reputation leaderboard", NameTranslationsProviderType = typeof(NameTranslationsProvider), DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider))]
    public async Task<InteractionCallback> LeaderboardAsync()
    {
        var (embed, component) = await LeaderboardHelper.CreateLeaderboardAsync(Context, serviceProvider, options, 0);
        return InteractionCallback.Message(new()
        {
            Embeds = [embed],
            Components = [component],
        });
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
