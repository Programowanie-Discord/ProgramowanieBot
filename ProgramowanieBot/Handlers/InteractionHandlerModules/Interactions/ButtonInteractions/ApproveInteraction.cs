using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ButtonInteractions;

public class ApproveInteraction : InteractionModule<ButtonInteractionContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigService _config;

    public ApproveInteraction(IServiceProvider serviceProvider, ConfigService config)
    {
        _serviceProvider = serviceProvider;
        _config = config;
    }

    [InteractionRequireUserChannelPermissions<ButtonInteractionContext>(Permissions.Administrator)]
    [Interaction("approve")]
    public async Task ApproveAsync(ulong helper, bool giveReputation, ulong? helper2 = null, bool? giveReputation2 = null)
    {
        var channelId = Context.Interaction.Channel.Id;
        await using (var context = _serviceProvider.GetRequiredService<DataContext>())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            if (await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved))
                throw new(_config.Interaction.PostAlreadyResolvedResponse);

            await PostsHelper.ResolvePostAsync(context, channelId);
            if (giveReputation)
                await ReputationHelper.AddReputationAsync(context, helper, 5);
            if (giveReputation2 == true)
                await ReputationHelper.AddReputationAsync(context, helper2.GetValueOrDefault(), 5);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        await RespondAsync(InteractionCallback.UpdateMessage(new()
        {
            Content = $"**{_config.Emojis.Success} {_config.Interaction.PostResolvedResponse}**",
            Components = Enumerable.Empty<ComponentProperties>(),
        }));
        var user = Context.User;
        var channel = (GuildThread)Context.Channel!;
        await channel.ModifyAsync(t =>
        {
            t.Archived = true;

            const int NameMaxLength = 100;
            var name = $"{_config.Interaction.PostResolvedPrefix} {channel.Name}";
            if (name.Length > NameMaxLength)
                name = name[..NameMaxLength];
            t.Name = name;
        }, new()
        {
            AuditLogReason = $"Approved by: {user.Username}#{user.Discriminator:D4} ({user.Id})",
        });
    }
}
