using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers;

[GatewayEvent(nameof(GatewayClient.GuildThreadCreate))]
internal class GuildThreadCreateHandler(GatewayClient client, Configuration configuration) : IGatewayEventHandler<GuildThreadCreateEventArgs>
{
    public async ValueTask HandleAsync(GuildThreadCreateEventArgs args)
    {
        if (!args.NewlyCreated || args.Thread is not PublicGuildThread thread)
            return;

        var guildThreadConfiguration = configuration.GuildThread;
        if (thread.ParentId != guildThreadConfiguration.HelpChannelId || !client.Cache.Guilds.TryGetValue(thread.GuildId, out var guild))
            return;

        var appliedTags = thread.AppliedTags;
        if (appliedTags != null)
        {
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
