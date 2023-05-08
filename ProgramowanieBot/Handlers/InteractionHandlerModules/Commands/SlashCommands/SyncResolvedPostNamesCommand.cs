using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.SlashCommands;

public class SyncResolvedPostNamesCommand : ApplicationCommandModule<SlashCommandContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigService _config;

    public SyncResolvedPostNamesCommand(IServiceProvider serviceProvider, ConfigService config)
    {
        _serviceProvider = serviceProvider;
        _config = config;
    }

    [SlashCommand(
        "sync-resolved-post-names",
        "Adds or changes prefix of resolved posts",
        DefaultGuildUserPermissions = Permissions.Administrator,
        NameTranslationsProviderType = typeof(NameTranslationsProvider),
        DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider))]
    public async Task SyncResolvedPostNamesAsync(
        [SlashCommandParameter(
            Name = "old_prefix",
            Description = "Old prefix, specify if you changed a prefix to a new one",
            NameTranslationsProviderType = typeof(OldPrefixNameTranslationsProvider),
            DescriptionTranslationsProviderType = typeof(OldPrefixDescriptionTranslationsProvider))]
        string? oldPrefix = null)
    {
        await RespondAsync(InteractionCallback.ChannelMessageWithSource($"**{_config.Emojis.Loading} {_config.Interaction.SyncingPostsResponse}**"));
        var oldPrefixWithSpace = $"{oldPrefix} ";
        var prefix = _config.Interaction.PostResolvedPrefix;
        var prefixWithSpace = $"{prefix} ";
        var helpChannelId = _config.GuildThread.HelpChannelId;
        var responseTask = Context.Interaction.GetResponseAsync();
        var postsTask = Context.Client.Rest.GetPublicArchivedGuildThreadsAsync(helpChannelId).ToDictionaryAsync(t => t.Id);
        var activePosts = await Context.Client.Rest.GetActiveGuildThreadsAsync(Context.Interaction.GuildId.GetValueOrDefault());
        var posts = await postsTask;

        var helpPosts = activePosts.Values.Where(t => t.ParentId == helpChannelId).ToArray();
        posts.EnsureCapacity(posts.Count + helpPosts.Length);
        foreach (var post in helpPosts)
            posts.TryAdd(post.Id, post);

        const int NameMaxLength = 100;
        ChangeNameDelegate changeName = oldPrefix != null ? ChangeNameWithOldPrefix : ChangeNameWithoutOldPrefix;

        await using (var context = _serviceProvider.GetRequiredService<DataContext>())
        {
            await foreach (var post in context.Posts.Where(p => p.IsResolved).AsAsyncEnumerable())
            {
                if (posts.TryGetValue(post.PostId, out var thread) && changeName(thread, out var name))
                {
                    await thread.ModifyAsync(t =>
                    {
                        t.Archived = t.Locked = false;
                        t.Name = name;
                    });
                    await thread.ModifyAsync(t => t.Archived = true);
                }
            }
        }
        var response = await responseTask;
        await response.ReplyAsync($"**{_config.Emojis.Success} {_config.Interaction.PostsSyncedResponse}**", failIfNotExists: false);

        bool ChangeNameWithOldPrefix(GuildThread guildThread, out string name)
        {
            name = guildThread.Name;
            if (name.StartsWith(oldPrefixWithSpace))
            {
                name = $"{prefixWithSpace}{name.AsSpan(oldPrefixWithSpace.Length)}";
                if (name.Length > NameMaxLength)
                    name = name[..NameMaxLength];
                return true;
            }
            else if (name.StartsWith(prefixWithSpace))
                return false;
            else
            {
                name = $"{prefixWithSpace}{name}";
                if (name.Length > NameMaxLength)
                    name = name[..NameMaxLength];
                return true;
            }
        }

        bool ChangeNameWithoutOldPrefix(GuildThread guildThread, out string name)
        {
            name = guildThread.Name;
            if (name.StartsWith(prefixWithSpace))
                return false;
            else
            {
                name = $"{prefixWithSpace}{name}";
                if (name.Length > NameMaxLength)
                    name = name[..NameMaxLength];
                return true;
            }
        }
    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "zsynchronizuj-nazwy-postów" },
        };
    }

    public class DescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Dodaje lub zmienia prefiks rozwiązanych postów" },
        };
    }

    public class OldPrefixNameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "stary_prefiks" },
        };
    }

    public class OldPrefixDescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Stary prefiks, podaj jeśli zmieniłeś prefiks na nowy" },
        };
    }

    private delegate bool ChangeNameDelegate(GuildThread guildThread, out string name);
}
