using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.SlashCommands;

public class SyncResolvedPostNamesCommand : ApplicationCommandModule<ExtendedSlashCommandContext>
{
    [SlashCommand(
        "sync-resolved-post-names",
        "Adds or changes prefix of resolved posts",
        DefaultGuildUserPermissions = Permissions.Administrator, NameTranslationsProviderType = typeof(NameTranslationsProvider),
        DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider))]
    public async Task SyncResolvedPostNamesAsync(
        [SlashCommandParameter(
            Name = "old_prefix",
            Description = "Old prefix, specify if you changed a prefix to a new one",
            NameTranslationsProviderType = typeof(OldPrefixNameTranslationsProvider),
            DescriptionTranslationsProviderType = typeof(OldPrefixDescriptionTranslationsProvider))]
        string? oldPrefix = null)
    {
        await RespondAsync(InteractionCallback.ChannelMessageWithSource($"**{Context.Config.Emojis.Loading} {Context.Config.Interaction.SyncingPostsResponse}**"));
        var oldPrefixWithSpace = $"{oldPrefix} ";
        var prefix = Context.Config.Interaction.PostResolvedPrefix;
        var prefixWithSpace = $"{prefix} ";
        var helpChannelId = Context.Config.GuildThread.HelpChannelId;
        var responseTask = Context.Interaction.GetResponseAsync();
        var postsTask = Context.Client.Rest.GetPublicArchivedGuildThreadsAsync(helpChannelId).ToDictionaryAsync(t => t.Id);
        var activePosts = await Context.Client.Rest.GetActiveGuildThreadsAsync(Context.Interaction.GuildId.GetValueOrDefault());
        var posts = await postsTask;

        posts.EnsureCapacity(posts.Count + activePosts.Count);
        foreach (var post in activePosts.Values.Where(t => t.ParentId == helpChannelId))
            posts.TryAdd(post.Id, post);

        const int NameMaxLength = 100;
        ChangeNameDelegate changeName = oldPrefix != null ? ChangeNameWithOldPrefix : ChangeNameWithoutOldPrefix;

        await using (var context = Context.Provider.GetRequiredService<DataContext>())
        {
            await foreach (var post in context.ResolvedPosts)
            {
                if (posts.TryGetValue(post.Id, out var thread) && changeName(thread, out var name))
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
        await response.ReplyAsync($"**{Context.Config.Emojis.Success} {Context.Config.Interaction.PostsSyncedResponse}**", failIfNotExists: false);

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
