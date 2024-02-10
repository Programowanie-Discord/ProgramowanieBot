using System.Net;

using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace ProgramowanieBot.Handlers;

[GatewayEvent(nameof(GatewayClient.GuildThreadCreate))]
internal class GuildThreadCreateHandler(GatewayClient client, IOptions<Configuration> options) : IGatewayEventHandler<GuildThreadCreateEventArgs>
{
    public async ValueTask HandleAsync(GuildThreadCreateEventArgs args)
    {
        if (!args.NewlyCreated || args.Thread is not PublicGuildThread thread)
            return;

        var configuration = options.Value;
        var guildThreadConfiguration = configuration.GuildThread;
        if (thread.ParentId != guildThreadConfiguration.HelpChannelId || !client.Cache.Guilds.TryGetValue(thread.GuildId, out var guild))
            return;

        var appliedTags = thread.AppliedTags;
        if (appliedTags is null)
            return;

        List<Role> roles = new(appliedTags.Count);
        foreach (var tag in appliedTags)
        {
            if (guildThreadConfiguration.HelpTagsRoles.TryGetValue(tag, out var roleId) && guild.Roles.TryGetValue(roleId, out var role))
                roles.Add(role);
        }

        List<ComponentProperties> components = new(2);
        ActionButtonProperties closeButton = new($"close:{thread.OwnerId}", guildThreadConfiguration.PostCloseButtonLabel, ButtonStyle.Danger);

        switch (roles.Count)
        {
            case 0:
                components.Add(new ActionRowProperties(
                [
                    closeButton,
                ]));
                break;
            case 1:
                var role = roles[0];
                components.Add(new ActionRowProperties(
                [
                    new ActionButtonProperties($"mention:{thread.OwnerId}:{role.Id}", role.Name, new(1035215423056134256), ButtonStyle.Success),
                    closeButton,
                ]));
                break;
            default:
                EmojiProperties emoji = new(1035215423056134256);
                components.Add(new StringMenuProperties($"mention:{thread.OwnerId}", roles.Select(r => new StringMenuSelectOptionProperties(r.Name, r.Id.ToString())
                {
                    Emoji = emoji,
                }))
                {
                    Placeholder = guildThreadConfiguration.MentionMenuPlaceholder,
                });
                goto case 0;
        }

        MessageProperties messageProperties = new()
        {
            Content = guildThreadConfiguration.HelpPostStartMessage,
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
            catch (RestException ex) when (ex.StatusCode is HttpStatusCode.Forbidden)
            {
                if (ex.Error is null or { Code: not 40058 })
                    throw;
            }
        }
        await thread.PinMessageAsync(message.Id);
    }
}
