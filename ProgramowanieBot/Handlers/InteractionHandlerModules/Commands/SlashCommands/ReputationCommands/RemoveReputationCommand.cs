using System.Globalization;

using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.SlashCommands.ReputationCommands;

public class RemoveReputationCommand : ApplicationCommandModule<ExtendedSlashCommandContext>
{
    [SlashCommand("remove-reputation", "Removes user reputation",
        NameTranslationsProviderType = typeof(NameTranslationsProvider),
        DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider),
        DefaultGuildUserPermissions = Permissions.Administrator)]
    public async Task RemoveReputationAsync(
        [SlashCommandParameter(
            Description = "User to remove reputation from",
            NameTranslationsProviderType = typeof(UserNameTranslationsProvider),
            DescriptionTranslationsProviderType = typeof(UserDescriptionTranslationsProvider))] User user,
        [SlashCommandParameter(Description = "Reputation to remove",
            NameTranslationsProviderType = typeof(ReputationNameTranslationsProvider),
            DescriptionTranslationsProviderType = typeof(ReputationDescriptionTranslationsProvider),
            MinValue = 1)] long reputation)
    {
        await using (var context = Context.Provider.GetRequiredService<DataContext>())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            await ReputationHelper.AddReputationAsync(context, user.Id, -reputation);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        await RespondAsync(InteractionCallback.ChannelMessageWithSource($"**{Context.Config.Emojis.Success} {reputation} of reputation was removed from {user}.**"));
    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "usuń-reputację" },
        };
    }

    public class DescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Usuwa reputację użytkownikowi" },
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
            { new("pl"), "Użytkownik, któremu chcesz usunąć reputację" },
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
            { new("pl"), "Reputacja do usunięcia" },
        };
    }
}
