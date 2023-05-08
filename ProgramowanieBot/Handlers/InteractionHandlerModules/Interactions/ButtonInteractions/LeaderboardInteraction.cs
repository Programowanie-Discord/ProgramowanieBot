using NetCord;
using NetCord.Services.Interactions;

using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ButtonInteractions;

public class LeaderboardInteraction : InteractionModule<ButtonInteractionContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigService _config;

    public LeaderboardInteraction(IServiceProvider serviceProvider, ConfigService config)
    {
        _serviceProvider = serviceProvider;
        _config = config;
    }

    [Interaction("leaderboard")]
    public async Task LeaderboardAsync(int page)
    {
        await RespondAsync(InteractionCallback.UpdateMessage(await LeaderboardHelper.CreateLeaderboardAsync(Context, _serviceProvider, _config, page)));
    }
}
