using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;
using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.ButtonInteractions;

public class PostCloseInteraction : InteractionModule<ButtonInteractionContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigService _config;

    public PostCloseInteraction(IServiceProvider serviceProvider, ConfigService config)
    {
        _serviceProvider = serviceProvider;
        _config = config;
    }

    [Interaction("close")]
    public async Task CloseAsync([AllowedUser<ButtonInteractionContext>] ulong threadOwnerId)
    {
        bool resolved;
        var channelId = Context.Interaction.Channel.Id;
        await using (var context = _serviceProvider.GetRequiredService<DataContext>())
            resolved = await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved);

        if (resolved)
        {
            await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
            {
                Content = $"**{_config.Emojis.Success} {_config.Interaction.PostClosedResponse}**",
                Flags = MessageFlags.Ephemeral,
            }));
            var user = Context.User;
            await Context.Client.Rest.ModifyGuildThreadAsync(channelId, c => c.Archived = true, new()
            {
                AuditLogReason = $"Closed by: {user.Username}#{user.Discriminator:D4} ({user.Id})",
            });
        }
        else
        {
            await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
            {
                Components = new ComponentProperties[]
                {
                    new UserMenuProperties("resolve")
                    {
                        Placeholder = _config.Interaction.SelectHelperMenuPlaceholder,
                        MaxValues = 2,
                    },
                    new ActionRowProperties(new ButtonProperties[]
                    {
                        new ActionButtonProperties($"resolve:{threadOwnerId}", _config.Interaction.IHelpedMyselfButtonLabel, ButtonStyle.Danger),
                    }),
                },
                Flags = MessageFlags.Ephemeral,
            }));
        }
    }
}
