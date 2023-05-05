using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers;

internal class ReactionHandler : BaseHandler<ConfigService.GuildThreadHandlerConfig>
{
    private record struct MessageAuthor(bool IsBot, ulong Id);

    private readonly ConcurrentDictionary<ulong, MessageAuthor> _messageAuthorsCache = new();

    public ReactionHandler(GatewayClient client, ILogger<ReactionHandler> logger, ConfigService config, IServiceProvider provider) : base(client, logger, config.GuildThread, provider)
    {
    }

    public override ValueTask StartAsync(CancellationToken cancellationToken)
    {
        Client.MessageReactionAdd += HandleReactionAddAsync;
        Client.MessageReactionRemove += HandleReactionRemoveAsync;
        return default;
    }

    public override ValueTask StopAsync(CancellationToken cancellationToken)
    {
        Client.MessageReactionAdd -= HandleReactionAddAsync;
        Client.MessageReactionRemove -= HandleReactionRemoveAsync;
        return default;
    }

    private async ValueTask HandleReactionAddAsync(MessageReactionAddEventArgs arg)
    {
        var channelId = arg.ChannelId;
        var userId = arg.UserId;
        if (userId == Client.Cache.User!.Id || !arg.Emoji.IsStandard || !IsHelpChannel(arg.GuildId.GetValueOrDefault(), channelId, out var thread) || userId == thread.OwnerId)
            return;

        var author = await GetAuthorAsync(channelId, arg.MessageId);
        var authorId = author.Id;
        if (author.IsBot || authorId == userId)
            return;

        switch (arg.Emoji.Name)
        {
            case "⬆️":
                {
                    await using var context = Provider.GetRequiredService<DataContext>();
                    await using var transaction = await context.Database.BeginTransactionAsync();
                    await ReputationHelper.AddReputationAsync(context, authorId, 1);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    break;
                }
            case "⬇️":
                {
                    await using var context = Provider.GetRequiredService<DataContext>();
                    await using var transaction = await context.Database.BeginTransactionAsync();
                    await ReputationHelper.AddReputationAsync(context, authorId, -1);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    break;
                }
        }
    }

    private async ValueTask HandleReactionRemoveAsync(MessageReactionRemoveEventArgs arg)
    {
        var channelId = arg.ChannelId;
        var userId = arg.UserId;
        if (userId == Client.Cache.User!.Id || !arg.Emoji.IsStandard || !IsHelpChannel(arg.GuildId.GetValueOrDefault(), channelId, out var thread) || userId == thread.OwnerId)
            return;

        var author = await GetAuthorAsync(channelId, arg.MessageId);
        var authorId = author.Id;
        if (author.IsBot || authorId == userId)
            return;

        switch (arg.Emoji.Name)
        {
            case "⬆️":
                {
                    await using var context = Provider.GetRequiredService<DataContext>();
                    await using var transaction = await context.Database.BeginTransactionAsync();
                    await ReputationHelper.AddReputationAsync(context, authorId, -1);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    break;
                }
            case "⬇️":
                {
                    await using var context = Provider.GetRequiredService<DataContext>();
                    await using var transaction = await context.Database.BeginTransactionAsync();
                    await ReputationHelper.AddReputationAsync(context, authorId, 1);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    break;
                }
        }
    }

    private bool IsHelpChannel(ulong guildId, ulong channelId, [NotNullWhen(true)][MaybeNullWhen(false)] out GuildThread? thread)
    {
        if (Client.Cache.Guilds.TryGetValue(guildId, out var guild))
        {
            if (guild.ActiveThreads.TryGetValue(channelId, out thread))
                return thread.ParentId == Config.HelpChannelId;
            else
            {
                Logger.LogWarning("Thread {channelId} was not found", channelId);
                return false;
            }
        }
        else
        {
            Logger.LogWarning("Guild {guildId} was not found", guildId);
            thread = null;
            return false;
        }
    }

    private async ValueTask<MessageAuthor> GetAuthorAsync(ulong channelId, ulong messageId)
    {
        if (_messageAuthorsCache.TryGetValue(messageId, out var author))
            return author;

        var message = await Client.Rest.GetMessageAsync(channelId, messageId);
        _messageAuthorsCache.TryAdd(messageId, author = new(message.Author.IsBot, message.Author.Id));
        return author;
    }
}
