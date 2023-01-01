using System.Diagnostics;

using Microsoft.EntityFrameworkCore;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Helpers;

internal static class ReputationHelper
{
    public static async Task AddReputationAsync(DataContext context, ulong userId, long value)
    {
        Debug.Assert(context.Database.CurrentTransaction != null, "Transaction is required.");

        var profile = await context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile != null)
        {
            profile.ReputationToday += value;
            profile.Reputation += value;
        }
        else
        {
            await context.Profiles.AddAsync(new()
            {
                UserId = userId,
                ReputationToday = value,
                Reputation = value,
            });
        }
    }

    public static async Task SetReputationAsync(DataContext context, ulong userId, long value)
    {
        Debug.Assert(context.Database.CurrentTransaction != null, "Transaction is required.");

        var profile = await context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile != null)
        {
            profile.ReputationToday += value - profile.Reputation;
            profile.Reputation = value;
        }
        else
        {
            await context.Profiles.AddAsync(new()
            {
                UserId = userId,
                ReputationToday = value,
                Reputation = value,
            });
        }
    }
}
