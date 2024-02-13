﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.InteractionHandlerModules.Interactions.ButtonInteractions;

public class ResolveInteraction(IServiceProvider serviceProvider, IOptions<Configuration> options) : InteractionModule<ButtonInteractionContext>
{
    [Interaction("resolve")]
    public async Task<InteractionCallback> ResolveAsync(ulong helperId)
    {
        var configuration = options.Value;

        var channel = Context.Channel;
        var channelId = channel.Id;
        await using (var context = serviceProvider.GetRequiredService<DataContext>())
            if (await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved))
                throw new(configuration.Interaction.PostAlreadyResolvedResponse);

        var closingMessage = await Context.Channel.SendMessageAsync(new()
        {
            Content = $"**{configuration.Emojis.Success} {string.Format(configuration.Interaction.WaitingForApprovalMessage, $"<@{helperId}>")}**",
            AllowedMentions = AllowedMentionsProperties.None,
        });

        await Context.Client.Rest.SendMessageAsync(configuration.Interaction.PostResolvedNotificationChannelId, new()
        {
            Content = $"**{configuration.Emojis.Success} {string.Format(configuration.Interaction.PostResolvedNotificationMessage, channel)}**",
            Components =
            [
                new ActionRowProperties(
                [
                    new ActionButtonProperties($"approve:{Context.Channel.Id}:{closingMessage.Id}:{helperId}:{helperId != Context.User.Id}::", configuration.Interaction.ApproveButtonLabel, ButtonStyle.Success),
                ]),
            ],
        });

        return InteractionCallback.DeferredModifyMessage;
    }
}