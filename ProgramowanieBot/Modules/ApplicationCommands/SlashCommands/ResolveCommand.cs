using System.Collections;
using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.InteractionHandlerModules.Commands.SlashCommands;

public class ResolveCommand(IServiceProvider serviceProvider, IOptions<Configuration> options) : ApplicationCommandModule<SlashCommandContext>
{
    [RequireThreadOwnerOfHelpChannel<SlashCommandContext>]
    [SlashCommand("resolve", "Closes your post and specifies who helped you", NameTranslationsProviderType = typeof(NameTranslationsProvider), DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider))]
    public async Task<InteractionCallback> ResolveAsync(
        [SlashCommandParameter(NameTranslationsProviderType = typeof(HelperNameTranslationsProvider), Description = "User who helped you", DescriptionTranslationsProviderType = typeof(HelperDescriptionTranslationsProvider))]
        [NoBot<SlashCommandContext>]
        User helper,
        [SlashCommandParameter(Name = "second_helper", NameTranslationsProviderType = typeof(Helper2NameTranslationsProvider), Description = "Another user who helped you", DescriptionTranslationsProviderType = typeof(Helper2DescriptionTranslationsProvider))]
        [NoBot<SlashCommandContext>]
        User? helper2 = null)
    {
        var configuration = options.Value;
        var channelId = Context.Channel.Id;

        await using (var context = serviceProvider.GetRequiredService<DataContext>())
            if (await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved))
                throw new(configuration.Interaction.PostAlreadyResolvedResponse);

        await PostsHelper.SendPostResolveMessagesAsync(channelId, Context.User.Id, helper.Id, helper2?.Id, Context.Client.Rest, configuration);

        return InteractionCallback.Message(new()
        {
            Content = configuration.Emojis.Success,
            Flags = MessageFlags.Ephemeral
        });

    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "rozwiązane" },
        };
    }

    public class DescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Zamyka twojego posta i wskazuje kto Ci pomógł" },
        };
    }

    public class HelperNameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "pomocnik" },
        };
    }

    public class HelperDescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Osoba, która Ci pomogła" },
        };
    }

    public class Helper2NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "drugi_pomocnik" },
        };
    }

    public class Helper2DescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Kolejna osoba, która Ci pomogła" },
        };
    }
}
