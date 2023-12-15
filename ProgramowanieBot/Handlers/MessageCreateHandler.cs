using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers;

[GatewayEvent(nameof(GatewayClient.MessageCreate))]
internal partial class MessageCreateHandler(GatewayClient client, Configuration configuration, IServiceProvider services) : IGatewayEventHandler<Message>
{
    private readonly TimeSpan _typingTimeout = TimeSpan.FromSeconds(configuration.GuildThread.ReactionTypingTimeoutSeconds);

    public ValueTask HandleAsync(Message message)
    {
        if (!message.Author.IsBot && message.Channel is PublicGuildThread thread && thread.ParentId == configuration.GuildThread.HelpChannelId)
            return HandleMessageInHelpChannelAsync(message, thread);
        return default;
    }

    private ValueTask HandleMessageInHelpChannelAsync(Message message, PublicGuildThread thread)
    {
        if (message.Id == thread.Id)
            return new(AddReactionsAsync());
        else
        {
            return new(Task.WhenAll(HandleMessageReactionsAsync(), HandleReminderAsync()));

            Task HandleMessageReactionsAsync()
            {
                TaskCompletionSource taskCompletionSource = new();

                Func<TypingStartEventArgs, ValueTask> handleTypingStartOnceAsync = null!;
                Func<Message, ValueTask> handleMessageCreateOnceAsync = null!;
                handleTypingStartOnceAsync = HandleTypingStartOnceAsync;
                handleMessageCreateOnceAsync = HandleMessageCreateOnceAsync;

                client.TypingStart += handleTypingStartOnceAsync;
                client.MessageCreate += handleMessageCreateOnceAsync;

                return taskCompletionSource.Task.WaitAsync(_typingTimeout).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        client.TypingStart -= handleTypingStartOnceAsync;
                        client.MessageCreate -= handleMessageCreateOnceAsync;
                        return AddReactionsAsync();
                    }
                    return Task.CompletedTask;
                });

                ValueTask HandleTypingStartOnceAsync(TypingStartEventArgs args)
                {
                    if (args.UserId != message.Author.Id || args.ChannelId != message.ChannelId)
                        return default;

                    client.TypingStart -= handleTypingStartOnceAsync;
                    client.MessageCreate -= handleMessageCreateOnceAsync;
                    taskCompletionSource.TrySetResult();
                    return default;
                }

                ValueTask HandleMessageCreateOnceAsync(Message newMessage)
                {
                    if (newMessage.Author.Id != message.Author.Id || newMessage.ChannelId != message.ChannelId)
                        return default;

                    client.TypingStart -= handleTypingStartOnceAsync;
                    client.MessageCreate -= handleMessageCreateOnceAsync;
                    taskCompletionSource.TrySetResult();
                    return default;
                }
            }

            Task HandleReminderAsync()
            {
                if (message.Author.Id != thread.OwnerId)
                    return Task.CompletedTask;

                var content = message.Content;
                if (configuration.GuildThread.PostResolveReminderKeywords.Any(k => content.Contains(k, StringComparison.InvariantCultureIgnoreCase)))
                    return SendReminderMessageAsync();

                return Task.CompletedTask;

                async Task SendReminderMessageAsync()
                {
                    bool reply;
                    await using (var context = services.GetRequiredService<DataContext>())
                    {
                        await using var transaction = await context.Database.BeginTransactionAsync();
                        var maxPostResolveReminders = configuration.GuildThread.MaxPostResolveReminders;
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
                            Content = configuration.GuildThread.PostResolveReminderMessage,
                            Components = new ActionRowProperties[]
                            {
                                new(new ButtonProperties[]
                                {
                                    new ActionButtonProperties($"close:{thread.OwnerId}", configuration.GuildThread.PostCloseButtonLabel, ButtonStyle.Danger),
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
