using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;
using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.ButtonInteractions;

public class PostCloseInteraction : InteractionModule<ExtendedButtonInteractionContext>
{
    [Interaction("close")]
    public async Task CloseAsync([AllowedUser<ExtendedButtonInteractionContext>] ulong threadOwnerId)
    {
        bool resolved;
        var channelId = Context.Interaction.ChannelId.GetValueOrDefault();
        await using (var context = Context.Provider.GetRequiredService<DataContext>())
            resolved = await context.ResolvedPosts.AnyAsync(p => p.Id == channelId);

        if (resolved)
        {
            await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
            {
                Content = $"**{Context.Config.Emojis.Success} {Context.Config.Interaction.PostClosedResponse}**",
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
                        Placeholder = Context.Config.Interaction.SelectHelperMenuPlaceholder,
                        MaxValues = 2,
                    },
                    new ActionRowProperties(new ButtonProperties[]
                    {
                        new ActionButtonProperties($"resolve:{threadOwnerId}", Context.Config.Interaction.IHelpedMyselfButtonLabel, ButtonStyle.Danger),
                    }),
                },
                Flags = MessageFlags.Ephemeral,
            }));
        }
    }
}
