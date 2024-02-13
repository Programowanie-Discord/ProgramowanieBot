using System.Diagnostics;

using Microsoft.EntityFrameworkCore;

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
}
