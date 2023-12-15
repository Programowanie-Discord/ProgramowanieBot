using System.Globalization;

using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.InteractionHandlerModules.Commands.SlashCommands.ReputationCommands;

public class SetReputationCommand(IServiceProvider serviceProvider, Configuration configuration) : ApplicationCommandModule<SlashCommandContext>
{
    [SlashCommand("set-reputation", "Sets user reputation",
        NameTranslationsProviderType = typeof(NameTranslationsProvider),
        DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider),
        DefaultGuildUserPermissions = Permissions.Administrator)]
    public async Task<InteractionCallback> SetReputationAsync(
        [SlashCommandParameter(
            Description = "User to set reputation for",
            NameTranslationsProviderType = typeof(UserNameTranslationsProvider),
            DescriptionTranslationsProviderType = typeof(UserDescriptionTranslationsProvider))] User user,
        [SlashCommandParameter(Description = "Reputation to set",
            NameTranslationsProviderType = typeof(ReputationNameTranslationsProvider),
            DescriptionTranslationsProviderType = typeof(ReputationDescriptionTranslationsProvider))] long reputation)
    {
        await using (var context = serviceProvider.GetRequiredService<DataContext>())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            await ReputationHelper.SetReputationAsync(context, user.Id, reputation);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        return InteractionCallback.Message($"**{configuration.Emojis.Success} {string.Format(configuration.Interaction.ReputationCommands.ReputationSetResponse, user, reputation)}**");
    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "ustaw-reputację" },
        };
    }

    public class DescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Ustawia reputację użytkownikowi" },
        };
    }

    public class UserNameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "użytkownik" },
        };
    }

    public class UserDescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Użytkownik, któremu chcesz ustawić reputację" },
        };
    }

    public class ReputationNameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "reputacja" },
        };
    }

    public class ReputationDescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Reputacja do ustawienia" },
        };
    }
}
