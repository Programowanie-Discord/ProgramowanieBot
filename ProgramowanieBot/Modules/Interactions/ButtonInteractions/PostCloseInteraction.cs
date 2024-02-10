using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.InteractionHandlerModules.Interactions.ButtonInteractions;

public class PostCloseInteraction(IServiceProvider serviceProvider, IOptions<Configuration> options) : InteractionModule<ButtonInteractionContext>
{
    [Interaction("close")]
    public async Task CloseAsync([AllowedUser<ButtonInteractionContext>] ulong threadOwnerId)
    {
        bool resolved;
        var channelId = Context.Interaction.Channel.Id;
        await using (var context = serviceProvider.GetRequiredService<DataContext>())
            resolved = await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved);

        var configuration = options.Value;

        if (resolved)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"**{configuration.Emojis.Success} {configuration.Interaction.PostClosedResponse}**",
                Flags = MessageFlags.Ephemeral,
            }));
            var user = Context.User;
            await Context.Client.Rest.ModifyGuildChannelAsync(channelId, c => c.Archived = true, new()
            {
                AuditLogReason = $"Closed by: {user.Username}#{user.Discriminator:D4} ({user.Id})",
            });
        }
        else
            await RespondAsync(InteractionCallback.Message(new()
            {
                Components =
                [
                    new UserMenuProperties("resolve")
                    {
                        Placeholder = configuration.Interaction.SelectHelperMenuPlaceholder,
                        MaxValues = 2,
                    },
                    new ActionRowProperties(
                    [
                        new ActionButtonProperties($"resolve:{threadOwnerId}", configuration.Interaction.IHelpedMyselfButtonLabel, ButtonStyle.Danger),
                    ]),
                ],
                Flags = MessageFlags.Ephemeral,
            }));
    }
}
