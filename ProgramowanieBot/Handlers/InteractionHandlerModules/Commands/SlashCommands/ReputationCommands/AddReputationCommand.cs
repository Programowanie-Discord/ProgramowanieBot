using System.Globalization;

using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.SlashCommands.ReputationCommands;

public class AddReputationCommand(IServiceProvider serviceProvider, ConfigService config) : ApplicationCommandModule<SlashCommandContext>
{
    [SlashCommand("add-reputation", "Adds user reputation",
        NameTranslationsProviderType = typeof(NameTranslationsProvider),
        DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider),
        DefaultGuildUserPermissions = Permissions.Administrator)]
    public async Task<InteractionCallback> AddReputationAsync(
        [SlashCommandParameter(
            Description = "User to add reputation to",
            NameTranslationsProviderType = typeof(UserNameTranslationsProvider),
            DescriptionTranslationsProviderType = typeof(UserDescriptionTranslationsProvider))] User user,
        [SlashCommandParameter(Description = "Reputation to add",
            NameTranslationsProviderType = typeof(ReputationNameTranslationsProvider),
            DescriptionTranslationsProviderType = typeof(ReputationDescriptionTranslationsProvider),
            MinValue = 1)] long reputation)
    {
        await using (var context = serviceProvider.GetRequiredService<DataContext>())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            await ReputationHelper.AddReputationAsync(context, user.Id, reputation);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        return InteractionCallback.Message($"**{config.Emojis.Success} {string.Format(config.Interaction.ReputationCommands.ReputationAddedResponse, user, reputation)}**");
    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "daj-reputację" },
        };
    }

    public class DescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Daje reputację użytkownikowi" },
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
            { new("pl"), "Użytkownik, któremu chcesz dać reputację" },
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
            { new("pl"), "Reputacja do dania" },
        };
    }
}
