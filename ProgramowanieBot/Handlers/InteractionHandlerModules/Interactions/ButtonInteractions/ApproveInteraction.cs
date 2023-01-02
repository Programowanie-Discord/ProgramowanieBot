using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ButtonInteractions;

public class ApproveInteraction : InteractionModule<ExtendedButtonInteractionContext>
{
    [InteractionRequireUserChannelPermissions<ExtendedButtonInteractionContext>(Permissions.Administrator)]
    [Interaction("approve")]
    public async Task ApproveAsync(ulong helperId)
    {
        var channelId = Context.Interaction.ChannelId.GetValueOrDefault();
        var user = Context.User;
        await using (var context = Context.Provider.GetRequiredService<DataContext>())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            if (await context.ResolvedPosts.AnyAsync(p => p.Id == channelId))
                throw new(Context.Config.Interaction.PostAlreadyResolvedResponse);

            await context.ResolvedPosts.AddAsync(new()
            {
                Id = channelId,
            });
            if (helperId != user.Id)
                await ReputationHelper.AddReputationAsync(context, helperId, 5);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        await RespondAsync(InteractionCallback.UpdateMessage(new()
        {
            Content = $"**{Context.Config.Emojis.Success} {Context.Config.Interaction.PostResolvedResponse}**",
            Components = Enumerable.Empty<ComponentProperties>(),
        }));
        await Context.Client.Rest.ModifyGuildThreadAsync(channelId, t => t.Archived = true, new()
        {
            AuditLogReason = $"Approved by: {user.Username}#{user.Discriminator:D4} ({user.Id})",
        });
    }
}
