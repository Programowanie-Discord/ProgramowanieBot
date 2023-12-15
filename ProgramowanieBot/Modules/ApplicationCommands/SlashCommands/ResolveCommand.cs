using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.InteractionHandlerModules.Commands.SlashCommands;

public class ResolveCommand(IServiceProvider serviceProvider, Configuration configuration) : ApplicationCommandModule<SlashCommandContext>
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
        var channel = Context.Channel;
        var channelId = channel.Id;
        await using (var context = serviceProvider.GetRequiredService<DataContext>())
            if (await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved))
                throw new(configuration.Interaction.PostAlreadyResolvedResponse);

        await Context.Client.Rest.SendMessageAsync(configuration.Interaction.PostResolvedNotificationChannelId, $"**{string.Format(configuration.Interaction.PostResolvedNotificationMessage, channel)}**");

        var isHelper2 = helper2 != null && helper != helper2;
        var user = Context.User;
        return InteractionCallback.Message(new()
        {
            Content = $"**{configuration.Emojis.Success} {(isHelper2 ? string.Format(configuration.Interaction.WaitingForApprovalWith2HelpersResponse, helper, helper2) : string.Format(configuration.Interaction.WaitingForApprovalResponse, helper))}**",
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"approve:{helper.Id}:{helper != user}:{(isHelper2 ? helper2!.Id : null)}:{(isHelper2 ? helper2 != user : null)}", configuration.Interaction.ApproveButtonLabel, ButtonStyle.Success),
                }),
            },
            AllowedMentions = AllowedMentionsProperties.None,
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
