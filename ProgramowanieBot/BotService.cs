using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace ProgramowanieBot;

internal class BotService : IHostedService
{
    public GatewayClient Client { get; }

    private readonly ILogger _logger;
    private readonly ulong _forumChannelId;
    private readonly IReadOnlyDictionary<ulong, ulong> _forumTagsRoles;
    private readonly string _forumPostStartMessage;

    public BotService(ILogger<BotService> logger, TokenService tokenService, IConfiguration configuration)
    {
        _logger = logger;
        _forumChannelId = ulong.Parse(configuration.GetRequiredSection("ForumChannelId").Value);
        _forumTagsRoles = configuration.GetRequiredSection("ForumTagsRoles").Get<IReadOnlyDictionary<string, string>>().ToDictionary(x => ulong.Parse(x.Key), x => ulong.Parse(x.Value));
        _forumPostStartMessage = configuration["ForumPostStartMessage"];

        Client = new(tokenService.Token, new()
        {
            Intents = GatewayIntent.Guilds | GatewayIntent.GuildUsers | GatewayIntent.GuildPresences,
        });
        Client.Log += message =>
        {
            _logger.Log(message.Severity switch
            {
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Error => LogLevel.Error,
                _ => LogLevel.Warning
            }, "{message} {description}", message.Message, message.Description ?? string.Empty);
            return default;
        };
        Client.GuildThreadCreate += HandleThreadCreateAsync;
    }

    private async ValueTask HandleThreadCreateAsync(GuildThreadCreateEventArgs args)
    {
        if (args.NewlyCreated && args.Thread is PublicGuildThread thread && thread.ParentId == _forumChannelId)
        {
            var appliedTags = thread.AppliedTags;
            if (appliedTags != null)
            {
                ulong roleId = default;
                if (appliedTags.Any(t => _forumTagsRoles.TryGetValue(t, out roleId)))
                {
                    var message = await SendStartMessageAsync($"{_forumPostStartMessage}\nPing: <@&{roleId}>");

                    if (message.Flags.HasFlag(MessageFlags.FailedToMentionSomeRolesInThread) && Client.Guilds.TryGetValue(args.Thread.GuildId, out var guild))
                    {
                        StringBuilder stringBuilder = new(2000, 2000);
                        List<Task> tasks = new(1);
                        foreach (var user in guild.Users.Values.Where(u => u.RoleIds.Contains(roleId)))
                        {
                            var mention = user.ToString();
                            if (stringBuilder.Length + mention.Length > 2000)
                            {
                                tasks.Add(SendAndDeleteMessageAsync(stringBuilder.ToString()));
                                stringBuilder.Clear();
                            }
                            stringBuilder.Append(mention);
                        }
                        if (stringBuilder.Length != 0)
                            tasks.Add(SendAndDeleteMessageAsync(stringBuilder.ToString()));

                        await Task.WhenAll(tasks);

                        async Task SendAndDeleteMessageAsync(string content)
                        {
                            var message = await thread.SendMessageAsync(content);
                            try
                            {
                                await message.DeleteAsync();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                else
                    await SendStartMessageAsync(_forumPostStartMessage);

                async Task<RestMessage> SendStartMessageAsync(MessageProperties messageProperties)
                {
                    RestMessage message;
                    while (true)
                    {
                        try
                        {
                            message = await thread.SendMessageAsync(messageProperties);
                            break;
                        }
                        catch (RestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            if ((await JsonDocument.ParseAsync(await ex.ResponseContent.ReadAsStreamAsync())).RootElement.GetProperty("code").GetInt32() != 40058)
                                throw;
                        }
                    }
                    return message;
                }
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Client.StartAsync();

    public Task StopAsync(CancellationToken cancellationToken) => Client.CloseAsync();
}
