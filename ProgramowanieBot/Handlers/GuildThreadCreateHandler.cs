using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers;

internal class GuildThreadCreateHandler : BaseHandler<GuildThreadServiceConfig>
{
    public GuildThreadCreateHandler(GatewayClient client, ILogger<GuildThreadCreateHandler> logger, ConfigService config) : base(client, logger, config.GuildThread)
    {
    }

    public override ValueTask StartAsync(CancellationToken cancellationToken)
    {
        Client.GuildThreadCreate += HandleGuildThreadCreateAsync;
        return default;
    }

    public override ValueTask StopAsync(CancellationToken cancellationToken)
    {
        Client.GuildThreadCreate -= HandleGuildThreadCreateAsync;
        return default;
    }

    private async ValueTask HandleGuildThreadCreateAsync(GuildThreadCreateEventArgs args)
    {
        if (args.NewlyCreated && args.Thread is PublicGuildThread thread && thread.ParentId == Config.ForumChannelId && Client.Guilds.TryGetValue(thread.GuildId, out var guild))
        {
            var appliedTags = thread.AppliedTags;
            if (appliedTags != null)
            {
                List<GuildRole> roles = new(appliedTags.Count);
                foreach (var tag in appliedTags)
                {
                    if (Config.ForumTagsRoles.TryGetValue(tag, out var roleId) && guild.Roles.TryGetValue(roleId, out var role))
                        roles.Add(role);
                }
                List<ComponentProperties> components = new(2);
                ActionButtonProperties closeButton = new($"close:{thread.OwnerId}", Config.PostCloseButtonLabel, ButtonStyle.Danger);
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
                            Placeholder = Config.MentionMenuPlaceholder,
                        });
                        goto case 0;
                }
                MessageProperties messageProperties = new()
                {
                    Content = Config.ForumPostStartMessage,
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
}
