using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers;

[GatewayEvent(nameof(GatewayClient.MessageReactionAdd))]
[GatewayEvent(nameof(GatewayClient.MessageReactionRemove))]
internal class ReactionHandler(GatewayClient client, ILogger<ReactionHandler> logger, IOptions<Configuration> options, IServiceProvider services) : IGatewayEventHandler<MessageReactionAddEventArgs>, IGatewayEventHandler<MessageReactionRemoveEventArgs>
{
    private record struct MessageAuthor(bool IsBot, ulong Id);

    private readonly ConcurrentDictionary<ulong, MessageAuthor> _messageAuthorsCache = new();

    private bool IsHelpChannel(ulong guildId, ulong channelId, [NotNullWhen(true)][MaybeNullWhen(false)] out GuildThread? thread)
    {
        if (client.Cache.Guilds.TryGetValue(guildId, out var guild))
        {
            if (guild.ActiveThreads.TryGetValue(channelId, out thread))
                return thread.ParentId == options.Value.GuildThread.HelpChannelId;
            else
            {
                logger.LogWarning("Thread {channelId} was not found", channelId);
                return false;
            }
        }
        else
        {
            logger.LogWarning("Guild {guildId} was not found", guildId);
            thread = null;
            return false;
        }
    }

    private async ValueTask<MessageAuthor> GetAuthorAsync(ulong channelId, ulong messageId)
    {
        if (_messageAuthorsCache.TryGetValue(messageId, out var author))
            return author;

        var message = await client.Rest.GetMessageAsync(channelId, messageId);
        _messageAuthorsCache.TryAdd(messageId, author = new(message.Author.IsBot, message.Author.Id));
        return author;
    }

    public async ValueTask HandleAsync(MessageReactionAddEventArgs args)
    {
        var channelId = args.ChannelId;
        var userId = args.UserId;
        if (userId == client.Id || args.Emoji.Id.HasValue || !IsHelpChannel(args.GuildId.GetValueOrDefault(), channelId, out var thread) || userId == thread.OwnerId)
            return;

        var author = await GetAuthorAsync(channelId, args.MessageId);
        var authorId = author.Id;
        if (author.IsBot || authorId == userId)
            return;

        switch (args.Emoji.Name)
        {
            case "⬆️":
                {
                    await using var context = services.GetRequiredService<DataContext>();
                    await using var transaction = await context.Database.BeginTransactionAsync();
                    await ReputationHelper.AddReputationAsync(context, authorId, 1);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    break;
                }
            case "⬇️":
                {
                    await using var context = services.GetRequiredService<DataContext>();
                    await using var transaction = await context.Database.BeginTransactionAsync();
                    await ReputationHelper.AddReputationAsync(context, authorId, -1);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    break;
                }
        }
    }

    public async ValueTask HandleAsync(MessageReactionRemoveEventArgs args)
    {
        var channelId = args.ChannelId;
        var userId = args.UserId;
        if (userId == client.Id || args.Emoji.Id.HasValue || !IsHelpChannel(args.GuildId.GetValueOrDefault(), channelId, out var thread) || userId == thread.OwnerId)
            return;

        var author = await GetAuthorAsync(channelId, args.MessageId);
        var authorId = author.Id;
        if (author.IsBot || authorId == userId)
            return;

        switch (args.Emoji.Name)
        {
            case "⬆️":
                {
                    await using var context = services.GetRequiredService<DataContext>();
                    await using var transaction = await context.Database.BeginTransactionAsync();
                    await ReputationHelper.AddReputationAsync(context, authorId, -1);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    break;
                }
            case "⬇️":
                {
                    await using var context = services.GetRequiredService<DataContext>();
                    await using var transaction = await context.Database.BeginTransactionAsync();
                    await ReputationHelper.AddReputationAsync(context, authorId, 1);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    break;
                }
        }
    }
}
