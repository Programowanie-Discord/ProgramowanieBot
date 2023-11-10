using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;
using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.ButtonInteractions;

public class PostCloseInteraction(IServiceProvider serviceProvider, ConfigService config) : InteractionModule<ButtonInteractionContext>
{
    [Interaction("close")]
    public async Task CloseAsync([AllowedUser<ButtonInteractionContext>] ulong threadOwnerId)
    {
        bool resolved;
        var channelId = Context.Interaction.Channel.Id;
        await using (var context = serviceProvider.GetRequiredService<DataContext>())
            resolved = await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved);

        if (resolved)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"**{config.Emojis.Success} {config.Interaction.PostClosedResponse}**",
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
            await RespondAsync(InteractionCallback.Message(new()
            {
                Components = new ComponentProperties[]
                {
                    new UserMenuProperties("resolve")
                    {
                        Placeholder = config.Interaction.SelectHelperMenuPlaceholder,
                        MaxValues = 2,
                    },
                    new ActionRowProperties(new ButtonProperties[]
                    {
                        new ActionButtonProperties($"resolve:{threadOwnerId}", config.Interaction.IHelpedMyselfButtonLabel, ButtonStyle.Danger),
                    }),
                },
                Flags = MessageFlags.Ephemeral,
            }));
        }
    }
}
