using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services;

using ProgramowanieBot.Data;
using ProgramowanieBot.Handlers.InteractionHandlerModules;

namespace ProgramowanieBot.Helpers;

internal static class LeaderboardHelper
{
    public static async Task<InteractionMessageProperties> CreateLeaderboardAsync<TContext>(TContext context, int page) where TContext : IExtendedContext, IUserContext, IGuildContext
    {
        string description;
        bool more;
        await using (var dataContext = context.Provider.GetRequiredService<DataContext>())
        {
            var users = context.Guild!.Users;
            var a = await dataContext.Profiles.OrderByDescending(p => p.Reputation).AsAsyncEnumerable().Where(p => users.ContainsKey(p.UserId)).Skip(page * 25).Take(26).Select(p => $"<@{p.UserId}>: {p.Reputation}").ToArrayAsync();
            var length = a.Length;
            description = string.Join('\n', a, 0, Math.Min(length, 25));
            more = length == 26;
        }
        var user = context.User;

        return new()
        {
            Embeds = new EmbedProperties[]
            {
                new()
                {
                    Title = context.Config.Interaction.ReputationCommands.LeaderboardEmbedTitle,
                    Description = description,
                    Footer = new()
                    {
                        Text = string.Format(context.Config.Interaction.ReputationCommands.LeaderboardEmbedFooter, $"{user.Username}#{user.Discriminator:D4}"),
                        IconUrl = (user.HasAvatar ? user.GetAvatarUrl() : user.DefaultAvatarUrl).ToString(),
                    },
                    Timestamp = DateTimeOffset.UtcNow,
                    Color = context.Config.EmbedColor,
                }
            },
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"leaderboard:{page - 1}", new EmojiProperties(context.Config.Emojis.Left), ButtonStyle.Secondary)
                    {
                        Disabled = page == 0,
                    },
                    new ActionButtonProperties($"leaderboard:{page + 1}", new EmojiProperties(context.Config.Emojis.Right), ButtonStyle.Secondary)
                    {
                        Disabled = !more,
                    },
                }),
            },
        };
    }
}
