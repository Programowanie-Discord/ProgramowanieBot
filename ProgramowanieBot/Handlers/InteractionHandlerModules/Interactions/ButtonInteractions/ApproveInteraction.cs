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
    public async Task ApproveAsync(ulong helper, bool giveReputation, ulong? helper2 = null, bool? giveReputation2 = null)
    {
        var channelId = Context.Interaction.ChannelId.GetValueOrDefault();
        await using (var context = Context.Provider.GetRequiredService<DataContext>())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            if (await context.ResolvedPosts.AnyAsync(p => p.Id == channelId))
                throw new(Context.Config.Interaction.PostAlreadyResolvedResponse);

            await context.ResolvedPosts.AddAsync(new()
            {
                Id = channelId,
            });
            if (giveReputation)
                await ReputationHelper.AddReputationAsync(context, helper, 5);
            if (giveReputation2 == true)
                await ReputationHelper.AddReputationAsync(context, helper2.GetValueOrDefault(), 5);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        await RespondAsync(InteractionCallback.UpdateMessage(new()
        {
            Content = $"**{Context.Config.Emojis.Success} {Context.Config.Interaction.PostResolvedResponse}**",
            Components = Enumerable.Empty<ComponentProperties>(),
        }));
        var user = Context.User;
        var channel = (GuildThread)Context.Channel!;
        await channel.ModifyAsync(t =>
        {
            t.Archived = true;

            const int NameMaxLength = 100;
            var name = $"{Context.Config.Interaction.PostResolvedPrefix} {channel.Name}";
            if (name.Length > NameMaxLength)
                name = name[..NameMaxLength];
            t.Name = name;
        }, new()
        {
            AuditLogReason = $"Approved by: {user.Username}#{user.Discriminator:D4} ({user.Id})",
        });
    }
}
