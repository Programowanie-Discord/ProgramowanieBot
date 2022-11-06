using System.Net.Http.Headers;

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
    private readonly string _mentionMenuPlaceholder;
    private readonly string _closePost;

    public BotService(ILogger<BotService> logger, TokenService tokenService, IConfiguration configuration)
    {
        _logger = logger;
        _forumChannelId = ulong.Parse(configuration.GetRequiredSection("ForumChannelId").Value);
        _forumTagsRoles = configuration.GetRequiredSection("ForumTagsRoles").Get<IReadOnlyDictionary<string, string>>().ToDictionary(x => ulong.Parse(x.Key), x => ulong.Parse(x.Value));
        _forumPostStartMessage = configuration["ForumPostStartMessage"];
        _mentionMenuPlaceholder = configuration["MentionMenuPlaceholder"];
        _closePost = configuration["ClosePost"];

        Client = new(tokenService.Token, new()
        {
            Intents = GatewayIntent.Guilds | GatewayIntent.GuildUsers | GatewayIntent.GuildPresences | GatewayIntent.GuildMessages | GatewayIntent.MessageContent,
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
        if (args.NewlyCreated && args.Thread is PublicGuildThread thread && thread.ParentId == _forumChannelId && Client.Guilds.TryGetValue(thread.GuildId, out var guild))
        {
            var appliedTags = thread.AppliedTags;
            if (appliedTags != null)
            {
                List<GuildRole> roles = new(appliedTags.Count);
                foreach (var tag in appliedTags)
                {
                    if (_forumTagsRoles.TryGetValue(tag, out var roleId) && guild.Roles.TryGetValue(roleId, out var role))
                        roles.Add(role);
                }
                List<ComponentProperties> components = new(2);
                ActionButtonProperties closeButton = new($"close:{thread.OwnerId}", _closePost, ButtonStyle.Danger);
                switch (roles.Count)
                {
                    case 0:
                        components.Add(new ActionRowProperties(new ButtonProperties[]
                        {
                            closeButton,
                        }));
                        break;
                    case 1:
                        var role = roles[0];
                        components.Add(new ActionRowProperties(new ButtonProperties[]
                        {
                            new ActionButtonProperties($"mention:{thread.OwnerId}:{role.Id}", role.Name, new(1035215423056134256), ButtonStyle.Success),
                            closeButton,
                        }));
                        break;
                    default:
                        EmojiProperties emoji = new(1035215423056134256);
                        components.Add(new StringMenuProperties($"mention:{thread.OwnerId}", roles.Select(r => new StringMenuSelectOptionProperties(r.Name, r.Id.ToString())
                        {
                            Emoji = emoji,
                        }))
                        {
                            Placeholder = _mentionMenuPlaceholder,
                        });
                        goto case 0;
                }
                MessageProperties messageProperties = new()
                {
                    Content = _forumPostStartMessage,
                    Components = components,
                };
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
                        if (await ex.GetDiscordStatusCodeAsync() != 40058)
                            throw;
                    }
                }
                await thread.PinMessageAsync(message.Id);
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Client.StartAsync();

    public Task StopAsync(CancellationToken cancellationToken) => Client.CloseAsync();
}
