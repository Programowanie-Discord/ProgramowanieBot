using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;

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
        if (message.Author.IsBot)
            return default;

        return HandleMessageInHelpChannelAsync(message);
    }

    private ValueTask HandleMessageInHelpChannelAsync(Message message)
    {
        if (message.Channel is PublicGuildThread thread && thread.ParentId == Config.HelpChannelId)
        {
            if (message.Id == thread.Id)
                return new(AddReactionsAsync());
            else
            {
                TaskCompletionSource taskCompletionSource = new();
                Client.TypingStart += HandleTypingStartOnceAsync;
                Client.MessageCreate += HandleMessageCreateOnceAsync;
                return new(taskCompletionSource.Task.WaitAsync(_typingTimeout).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Client.TypingStart -= HandleTypingStartOnceAsync;
                        Client.MessageCreate -= HandleMessageCreateOnceAsync;
                        return AddReactionsAsync();
                    }
                    return Task.CompletedTask;
                }));

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
        }
        return default;
    }
}
