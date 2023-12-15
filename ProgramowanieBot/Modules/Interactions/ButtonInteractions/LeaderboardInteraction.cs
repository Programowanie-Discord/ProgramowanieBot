using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.InteractionHandlerModules.Interactions.ButtonInteractions;

public class LeaderboardInteraction(IServiceProvider serviceProvider, Configuration configuration) : InteractionModule<ButtonInteractionContext>
{
    [Interaction("leaderboard")]
    public async Task<InteractionCallback> LeaderboardAsync(int page)
    {
        var (embed, component) = await LeaderboardHelper.CreateLeaderboardAsync(Context, serviceProvider, configuration, page);
        return InteractionCallback.ModifyMessage(m => (m.Embeds, m.Components) = ([embed], [component]));
    }
}
