using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace ProgramowanieBot.Handlers;

internal partial class MessageHandler : BaseHandler<GuildThreadHandlerConfig>
{
    private readonly HttpClient _httpClient;
    private static readonly TimeSpan _typingTimeout = TimeSpan.FromSeconds(6);

    public MessageHandler(GatewayClient client, ILogger<MessageHandler> logger, HttpClient httpClient, ConfigService config, IServiceProvider provider) : base(client, logger, config.GuildThread, provider)
    {
        _httpClient = httpClient;
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

    private async ValueTask HandleMessageCreateAsync(Message message)
    {
        if (message.Author.IsBot)
            return;

        var task = HandleMessageInHelpChannelAsync(message);
        await HandleGitHubCodeAsync(message);
        await task;
    }

    private async ValueTask HandleMessageInHelpChannelAsync(Message message)
    {
        if (message.Channel is PublicGuildThread thread && thread.ParentId == Config.HelpChannelId)
        {
            TaskCompletionSource taskCompletionSource = new();
            Client.TypingStart += HandleTypingStartOnceAsync;
            Client.MessageCreate += HandleMessageCreateOnceAsync;
            await taskCompletionSource.Task.WaitAsync(_typingTimeout).ContinueWith(async task =>
            {
                if (task.IsFaulted)
                {
                    Client.TypingStart -= HandleTypingStartOnceAsync;
                    Client.MessageCreate -= HandleMessageCreateOnceAsync;
                    await message.AddReactionAsync("⬆️");
                    await message.AddReactionAsync("⬇️");
                }
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
    }

    private async ValueTask HandleGitHubCodeAsync(Message message)
    {
        var matches = GetGitHubRegex().Matches(message.Content);
        if (matches.Count == 0)
            return;

        StringBuilder stringBuilder = new(2000, 2000);
        foreach (var match in (IList<Match>)matches)
        {
            var groups = match.Groups;
            var path2 = groups["path2"];

            var url = $"https://github.com/{groups["path1"]}/raw/{path2}";
            string fileContent;
            try
            {
                fileContent = await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to download GitHub file content, url: {url}", url);
                return;
            }

            var line1Group = groups["line1"];
            var line2Group = groups["line2"];

            var array = fileContent.Split('\n');
            const int maxLines = 15;
            int index, count;
            if (line1Group.Success)
            {
                if (!int.TryParse(line1Group.ValueSpan, out var line1))
                    continue;

                if (line1 > array.Length)
                    line1 = 0;

                if (line2Group.Success)
                {
                    if (!int.TryParse(line2Group.ValueSpan, out var line2))
                        continue;

                    if (line2 > array.Length)
                        line2 = maxLines;

                    int diff;
                    if (line1 <= line2)
                    {
                        index = line1;
                        diff = line2 - line1;
                    }
                    else
                    {
                        index = line2;
                        diff = line1 - line2;
                    }
                    if (diff >= maxLines)
                        count = maxLines;
                    else
                        count = diff + 1;
                }
                else
                {
                    index = Math.Max(line1 - (maxLines / 2), 0);
                    count = maxLines;
                }
                if (index != 0)
                    index--;
            }
            else
            {
                index = 0;
                count = maxLines;
            }

            int startLength = stringBuilder.Length;
            if (!AppendFileName())
                goto Revert;

            if (!EnsureCapacity(4))
                goto Revert;
            stringBuilder.Append('\n');
            stringBuilder.Append("```");

            if (!AppendExtension())
                goto Revert;

            int length = 0;
            for (int maxLine = index + count; maxLine > 0; maxLine /= 10)
                length++;

            int i = 0;
            foreach (var l in array.Skip(index).Take(count))
            {
                i++;
                var s = $"{(index + i).ToString().PadLeft(length)}\t{l.Replace("``", "`\u00AD`")}";
                if (!EnsureCapacity(s.Length))
                {
                    stringBuilder.Append("```");
                    goto Break;
                }
                stringBuilder.Append(s);
                if (!EnsureCapacity(1))
                    goto Break;
                stringBuilder.Append('\n');
            }
            stringBuilder.Append("```");

            bool EnsureCapacity(int length) => stringBuilder.Length + length <= 2000 - 3;

            bool AppendFileName()
            {
                var span = path2.ValueSpan;
                var name = span[(span.LastIndexOf('/') + 1)..];

                if (!EnsureCapacity(name.Length + 4))
                    return false;

                stringBuilder.Append("**");
                stringBuilder.Append(name);
                stringBuilder.Append("**");
                return true;
            }

            bool AppendExtension()
            {
                var path2Value = path2.ValueSpan;
                var lastIndex = path2Value.LastIndexOf('.');
                if (lastIndex != -1)
                {
                    var start = lastIndex + 1;
                    if (start != path2Value.Length)
                    {
                        if (!EnsureCapacity(path2Value.Length - lastIndex))
                            return false;

                        Span<char> lower = new char[path2Value.Length - start];
                        path2Value[start..].ToLowerInvariant(lower);
                        stringBuilder.Append(lower);
                        stringBuilder.Append('\n');
                        return true;
                    }
                }

                if (!EnsureCapacity(1))
                    return false;
                stringBuilder.Append('\n');
                return true;
            }
            continue;
            Revert:
            stringBuilder.Length = startLength;
            break;
        }
        Break:
        if (stringBuilder.Length != 0)
            await Client.Rest.SendMessageAsync(message.ChannelId, new()
            {
                Content = stringBuilder.ToString(),
                AllowedMentions = AllowedMentionsProperties.None,
            });
    }

    [GeneratedRegex(@"github\.com/(?<path1>[a-zA-Z\d-]+/[\w.-]+)/blob/(?<path2>[\w/\.%-]+)((#L(?<line1>\d+))(-L(?<line2>\d+))?)?")]
    private static partial Regex GetGitHubRegex();
}
