using System.Diagnostics;

using Microsoft.EntityFrameworkCore;

using NetCord;
using NetCord.Rest;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Helpers;

internal static class PostsHelper
{
    public static async Task ResolvePostAsync(DataContext context, ulong channelId)
    {
        Debug.Assert(context.Database.CurrentTransaction != null, "Transaction is required.");

        var post = await context.Posts.FirstOrDefaultAsync(p => p.PostId == channelId);
        if (post == null)
            await context.Posts.AddAsync(new()
            {
                PostId = channelId,
                PostResolveReminderCounter = 0,
                IsResolved = true,
            });
        else
            post.IsResolved = true;
    }

    public static async Task IncrementPostResolveReminderCounterAsync(DataContext context, ulong channelId)
    {
        Debug.Assert(context.Database.CurrentTransaction != null, "Transaction is required.");

        var post = await context.Posts.FirstOrDefaultAsync(p => p.PostId == channelId);
        if (post == null)
            await context.Posts.AddAsync(new()
            {
                PostId = channelId,
                PostResolveReminderCounter = 1,
                IsResolved = false,
            });
        else
            post.PostResolveReminderCounter++;
    }

    public static async Task SendPostResolveMessagesAsync(ulong channelId, ulong userId, ulong helperId, ulong? helper2Id, RestClient rest, Configuration configuration)
    {;
        var isHelper2 = helper2Id != null && helperId != helper2Id;
        var closingMessage = await rest.SendMessageAsync(channelId, new()
        {
            Content = $"**{configuration.Emojis.Success} {(isHelper2 ? string.Format(configuration.Interaction.WaitingForApprovalWith2HelpersMessage, $"<@{helperId}>", $"<@{helper2Id}>") : string.Format(configuration.Interaction.WaitingForApprovalMessage, $"<@{helperId}>"))}**",
            AllowedMentions = AllowedMentionsProperties.None,
        });

        await rest.SendMessageAsync(configuration.Interaction.PostResolvedNotificationChannelId, new()
        {
            Content = $"**{string.Format(configuration.Interaction.PostResolvedNotificationMessage, $"<#{channelId}>")}**",
            Components =
            [
                new ActionRowProperties(
                [
                    new ActionButtonProperties($"approve:{channelId}:{closingMessage.Id}:{helperId}:{helperId != userId}:{(isHelper2 ? helper2Id : null)}:{(isHelper2 ? helper2Id != userId : null)}", configuration.Interaction.ApproveButtonLabel, ButtonStyle.Success),
                ]),
            ],
        });
    }
}
