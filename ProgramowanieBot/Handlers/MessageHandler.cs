using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

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
            return new(HandleMessageInHelpChannelAsync(message, thread));
        return default;
    }

    private Task HandleMessageInHelpChannelAsync(Message message, PublicGuildThread thread)
    {
        if (message.Id == thread.Id)
            return AddReactionsAsync();
        else
        {
            return Task.WhenAll(HandleMessageReactionsAsync(), HandleReminderAsync());

            Task HandleMessageReactionsAsync()
            {
                TaskCompletionSource taskCompletionSource = new();

                Func<TypingStartEventArgs, ValueTask> handleTypingStartOnceAsync = null!;
                Func<Message, ValueTask> handleMessageCreateOnceAsync = null!;
                handleTypingStartOnceAsync = HandleTypingStartOnceAsync;
                handleMessageCreateOnceAsync = HandleMessageCreateOnceAsync;

                Client.TypingStart += handleTypingStartOnceAsync;
                Client.MessageCreate += handleMessageCreateOnceAsync;

                return taskCompletionSource.Task.WaitAsync(_typingTimeout).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Client.TypingStart -= handleTypingStartOnceAsync;
                        Client.MessageCreate -= handleMessageCreateOnceAsync;
                        return AddReactionsAsync();
                    }
                    return Task.CompletedTask;
                });

                ValueTask HandleTypingStartOnceAsync(TypingStartEventArgs args)
                {
                    if (args.UserId != message.Author.Id || args.ChannelId != message.ChannelId)
                        return default;

                    Client.TypingStart -= handleTypingStartOnceAsync;
                    Client.MessageCreate -= handleMessageCreateOnceAsync;
                    taskCompletionSource.TrySetResult();
                    return default;
                }

                ValueTask HandleMessageCreateOnceAsync(Message newMessage)
                {
                    if (newMessage.Author.Id != message.Author.Id || newMessage.ChannelId != message.ChannelId)
                        return default;

                    Client.TypingStart -= handleTypingStartOnceAsync;
                    Client.MessageCreate -= handleMessageCreateOnceAsync;
                    taskCompletionSource.TrySetResult();
                    return default;
                }
            }

            Task HandleReminderAsync()
            {
                if (message.Author.Id != thread.OwnerId)
                    return Task.CompletedTask;

                var content = message.Content;
               if (Config.PostResolveReminderKeywords.Any(k => content.Contains(k, StringComparison.InvariantCultureIgnoreCase)))
                    return SendReminderMessageAsync();

                return Task.CompletedTask;

                async Task SendReminderMessageAsync()
                {
                    bool reply;
                    await using (var context = Provider.GetRequiredService<DataContext>())
                    {
                        await using var transaction = await context.Database.BeginTransactionAsync();
                        var maxPostResolveReminders = Config.MaxPostResolveReminders;
                        if (!await context.Posts.AnyAsync(p => p.PostId == message.ChannelId && (p.IsResolved || p.PostResolveReminderCounter >= maxPostResolveReminders)))
                        {
                            await PostsHelper.IncrementPostResolveReminderCounterAsync(context, message.ChannelId);
                            await context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            reply = true;
                        }
                        else
                            reply = false;
                    }

                    if (reply)
                    {
                        await thread.SendMessageAsync(new()
                        {
                            Content = Config.PostResolveReminderMessage,
                            Components = new ActionRowProperties[]
                            {
                                new(new ButtonProperties[]
                                {
                                    new ActionButtonProperties($"close:{thread.OwnerId}", Config.PostCloseButtonLabel, ButtonStyle.Danger),
                                }),
                            },
                            MessageReference = new(message.Id),
                        });
                    }
                }
            }
        }

        async Task AddReactionsAsync()
        {
            await message.AddReactionAsync("⬆️");
            await message.AddReactionAsync("⬇️");
        }
    }
}
