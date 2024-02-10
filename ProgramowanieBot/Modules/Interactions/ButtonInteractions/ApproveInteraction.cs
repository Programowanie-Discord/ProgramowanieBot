using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.InteractionHandlerModules.Interactions.ButtonInteractions;

public class ApproveInteraction(IServiceProvider serviceProvider, IOptions<Configuration> options) : InteractionModule<ButtonInteractionContext>
{
    [RequireUserPermissions<ButtonInteractionContext>(Permissions.Administrator)]
    [Interaction("approve")]
    public async Task ApproveAsync(ulong helper, bool giveReputation, ulong? helper2 = null, bool? giveReputation2 = null)
    {
        var configuration = options.Value;

        var channel = (GuildThread)Context.Channel;
        var channelId = channel.Id;
        await using (var context = serviceProvider.GetRequiredService<DataContext>())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            if (await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved))
                throw new(configuration.Interaction.PostAlreadyResolvedResponse);

            await PostsHelper.ResolvePostAsync(context, channelId);
            if (giveReputation)
                await ReputationHelper.AddReputationAsync(context, helper, 5);
            if (giveReputation2 == true)
                await ReputationHelper.AddReputationAsync(context, helper2.GetValueOrDefault(), 5);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        await RespondAsync(InteractionCallback.ModifyMessage(m =>
        {
            m.Content = $"**{configuration.Emojis.Success} {configuration.Interaction.PostResolvedResponse}**";
            m.Components = [];
        }));

        var user = Context.User;
        await channel.ModifyAsync(t =>
        {
            t.Archived = true;

            const int NameMaxLength = 100;
            var name = $"{configuration.Interaction.PostResolvedPrefix} {channel.Name}";
            if (name.Length > NameMaxLength)
                name = name[..NameMaxLength];
            t.Name = name;
        }, new()
        {
            AuditLogReason = $"Approved by: {user.Username}#{user.Discriminator:D4} ({user.Id})",
        });
    }
}
