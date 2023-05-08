using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ButtonInteractions;

public class ResolveInteraction : InteractionModule<ButtonInteractionContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigService _config;

    public ResolveInteraction(IServiceProvider serviceProvider, ConfigService config)
    {
        _serviceProvider = serviceProvider;
        _config = config;
    }

    [Interaction("resolve")]
    public async Task ResolveAsync(ulong helper)
    {
        var channelId = Context.Interaction.Channel.Id;
        await using (var context = _serviceProvider.GetRequiredService<DataContext>())
        {
            if (await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved))
                throw new(_config.Interaction.PostAlreadyResolvedResponse);
        }

        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Content = $"**{_config.Emojis.Success} {string.Format(_config.Interaction.WaitingForApprovalResponse, $"<@{helper}>")}**",
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"approve:{helper}:{helper != Context.User.Id}::", _config.Interaction.ApproveButtonLabel, ButtonStyle.Success),
                }),
            },
            AllowedMentions = AllowedMentionsProperties.None,
        }));
    }
}
