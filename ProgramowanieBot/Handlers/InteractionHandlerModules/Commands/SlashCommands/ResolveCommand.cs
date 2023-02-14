using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;
using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.SlashCommands;

public class ResolveCommand : ApplicationCommandModule<ExtendedSlashCommandContext>
{
    [RequireThreadOwnerOfHelpChannel<ExtendedSlashCommandContext>]
    [SlashCommand("resolve", "Closes your post and specifies who helped you", NameTranslationsProviderType = typeof(NameTranslationsProvider), DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider))]
    public async Task ResolveAsync([SlashCommandParameter(NameTranslationsProviderType = typeof(HelperNameTranslationsProvider), Description = "User who helped you", DescriptionTranslationsProviderType = typeof(HelperDescriptionTranslationsProvider))][NoBot<ExtendedSlashCommandContext>] User helper)
    {
        var channelId = Context.Interaction.ChannelId.GetValueOrDefault();
        await using (var context = Context.Provider.GetRequiredService<DataContext>())
        {
            if (await context.ResolvedPosts.AnyAsync(p => p.Id == channelId))
                throw new(Context.Config.Interaction.PostAlreadyResolvedResponse);
        }

        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Content = $"**{Context.Config.Emojis.Success} {string.Format(Context.Config.Interaction.WaitingForApprovalResponse, helper)}**",
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"approve:{helper.Id}:{helper != Context.User}", Context.Config.Interaction.ApproveButtonLabel, ButtonStyle.Success),
                }),
            },
            AllowedMentions = AllowedMentionsProperties.None,
        }));
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
}
