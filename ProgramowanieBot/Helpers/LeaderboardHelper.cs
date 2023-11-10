using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Helpers;

internal static class LeaderboardHelper
{
    public record struct Leaderboard(EmbedProperties Embed, ComponentProperties Component);

    public static async Task<Leaderboard> CreateLeaderboardAsync<TContext>(TContext context, IServiceProvider serviceProvider, ConfigService config, int page) where TContext : IUserContext, IGuildContext
    {
        string description;
        bool more;
        await using (var dataContext = serviceProvider.GetRequiredService<DataContext>())
        {
            var users = context.Guild!.Users;
            var a = await dataContext.Profiles.OrderByDescending(p => p.Reputation).AsAsyncEnumerable().Where(p => users.ContainsKey(p.UserId)).Skip(page * 25).Take(26).Select(p => $"<@{p.UserId}>: {p.Reputation}").ToArrayAsync();
            var length = a.Length;
            description = string.Join('\n', a, 0, Math.Min(length, 25));
            more = length == 26;
        }
        var user = context.User;

        EmbedProperties embed = new()
        {
            Title = config.Interaction.ReputationCommands.LeaderboardEmbedTitle,
            Description = description,
            Footer = new()
            {
                Text = string.Format(config.Interaction.ReputationCommands.LeaderboardEmbedFooter, $"{user.Username}#{user.Discriminator:D4}"),
                IconUrl = (user.HasAvatar ? user.GetAvatarUrl() : user.DefaultAvatarUrl).ToString(),
            },
            Timestamp = DateTimeOffset.UtcNow,
            Color = config.EmbedColor,
        };

        ActionRowProperties component = new(new ButtonProperties[]
        {
            new ActionButtonProperties($"leaderboard:{page - 1}", new EmojiProperties(config.Emojis.Left), ButtonStyle.Secondary)
            {
                Disabled = page == 0,
            },
            new ActionButtonProperties($"leaderboard:{page + 1}", new EmojiProperties(config.Emojis.Right), ButtonStyle.Secondary)
            {
                Disabled = !more,
            },
        });

        return new(embed, component);
    }
}
