using NetCord;
using NetCord.Services.Interactions;

using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ButtonInteractions;

public class LeaderboardInteraction : InteractionModule<ExtendedButtonInteractionContext>
{
    [Interaction("leaderboard")]
    public async Task LeaderboardAsync(int page)
    {
        await RespondAsync(InteractionCallback.UpdateMessage(await LeaderboardHelper.CreateLeaderboardAsync(Context, page)));
    }
}
