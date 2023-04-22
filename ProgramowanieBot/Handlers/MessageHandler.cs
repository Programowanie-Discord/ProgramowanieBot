using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers;

internal partial class MessageHandler : BaseHandler<ConfigService.GuildThreadHandlerConfig>
{
    private readonly TimeSpan _typingTimeout;

    public MessageHandler(GatewayClient client, ILogger<MessageHandler> logger, ConfigService config, IServiceProvider provider) : base(client, logger, config.GuildThread, provider)
    {
        _typingTimeout = TimeSpan.FromSeconds(config.GuildThread.ReactionTypingTimeoutSeconds);
    }

    public override ValueTask StartAsync(CancellationToken cancellationToken)
    {
        Client.MessageCreate += HandleMessageCreateAsync;
        return default;
    }

    public override ValueTask StopAsync(CancellationToken cancellationToken)
    {
        Client.MessageCreate -= HandleMessageCreateAsync;
        return default;
    }

    private ValueTask HandleMessageCreateAsync(Message message)
    {
        if (!message.Author.IsBot && message.Channel is PublicGuildThread thread && thread.ParentId == Config.HelpChannelId)
            return HandleMessageInHelpChannelAsync(message, thread);
        return default;
    }

    private async ValueTask HandleMessageInHelpChannelAsync(Message message, PublicGuildThread thread)
    {
        if (message.Id == thread.Id)
        {
            await AddReactionsAsync();
            return;
        }
        else
        {
            if (message.Author.Id == thread.OwnerId)
                foreach (string keyword in Config.PostResolveReminderKeywords)
                {
                    if (message.Content.Contains(keyword, StringComparison.InvariantCultureIgnoreCase))
                    {
                        await SendReminderMessageAsync();
                        break;
                    }
                }

            TaskCompletionSource taskCompletionSource = new();
            Client.TypingStart += HandleTypingStartOnceAsync;
            Client.MessageCreate += HandleMessageCreateOnceAsync;
            await taskCompletionSource.Task.WaitAsync(_typingTimeout).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Client.TypingStart -= HandleTypingStartOnceAsync;
                    Client.MessageCreate -= HandleMessageCreateOnceAsync;
                    return AddReactionsAsync();
                }
                return Task.CompletedTask;
            });

            ValueTask HandleTypingStartOnceAsync(TypingStartEventArgs args)
            {
                if (args.UserId != message.Author.Id || args.ChannelId != message.ChannelId)
                    return default;

                Client.TypingStart -= HandleTypingStartOnceAsync;
                Client.MessageCreate -= HandleMessageCreateOnceAsync;
                taskCompletionSource.TrySetResult();
                return default;
            }

            ValueTask HandleMessageCreateOnceAsync(Message newMessage)
            {
                if (newMessage.Author.Id != message.Author.Id || newMessage.ChannelId != message.ChannelId)
                    return default;

                Client.TypingStart -= HandleTypingStartOnceAsync;
                Client.MessageCreate -= HandleMessageCreateOnceAsync;
                taskCompletionSource.TrySetResult();
                return default;
            }
        }

        async Task AddReactionsAsync()
        {
            await message.AddReactionAsync("⬆️");
            await message.AddReactionAsync("⬇️");
        }

        async Task SendReminderMessageAsync()
        {
            await using var context = Provider.GetRequiredService<DataContext>();
            await using var transaction = await context.Database.BeginTransactionAsync(default);
            var maxPostResolveReminders = Config.MaxPostResolveReminders;
            if (!await context.Posts.AnyAsync(p => p.PostId == message.ChannelId && (p.IsResolved || p.PostResolveReminderCounter >= maxPostResolveReminders)))
            {
                await message.ReplyAsync(Config.PostResolveReminderMessage);
                await PostsHelper.IncrementPostResolveReminderCounterAsync(context, message.ChannelId);
            }
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}
