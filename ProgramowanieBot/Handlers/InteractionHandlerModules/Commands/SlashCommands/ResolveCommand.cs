using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;
using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.SlashCommands;

public class ResolveCommand : ApplicationCommandModule<ExtendedSlashCommandContext>
{
    [RequireThreadOwnerOfHelpChannel<ExtendedSlashCommandContext>]
    [SlashCommand("resolve", "Closes your post and specifies who helped you", NameTranslationsProviderType = typeof(NameTranslationsProvider), DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider))]
    public async Task ResolveAsync([SlashCommandParameter(NameTranslationsProviderType = typeof(HelperNameTranslationsProvider), Description = "User who helped you", DescriptionTranslationsProviderType = typeof(HelperDescriptionTranslationsProvider))][NoBotAttribute<ExtendedSlashCommandContext>] User helper)
    {
        var thread = (GuildThread)Context.Channel!;
        await using (var context = Context.Provider.GetRequiredService<DataContext>())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            if (await context.ResolvedPosts.AnyAsync(p => p.Id == thread.Id))
                throw new(Context.Config.Interaction.PostAlreadyResolved);

            await context.ResolvedPosts.AddAsync(new()
            {
                Id = Context.Interaction.ChannelId.GetValueOrDefault(),
            });
            if (helper.Id != Context.User.Id)
                await ReputationHelper.AddReputationAsync(context, helper.Id, 5);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        await RespondAsync(InteractionCallback.ChannelMessageWithSource($"**{Context.Config.Emojis.Success} {Context.Config.Interaction.PostResolvedResponse}**"));
        await thread.ModifyAsync(t => t.Archived = t.Locked = true);
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
