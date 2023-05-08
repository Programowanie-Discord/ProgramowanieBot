using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.UserMenuInteractions;

public class ResolveInteraction : InteractionModule<UserMenuInteractionContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigService _config;

    public ResolveInteraction(IServiceProvider serviceProvider, ConfigService config)
    {
        _serviceProvider = serviceProvider;
        _config = config;
    }

    [Interaction("resolve")]
    public async Task ResolveAsync()
    {
        var channelId = Context.Interaction.Channel.Id;
        await using (var context = _serviceProvider.GetRequiredService<DataContext>())
        {
            if (await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved))
                throw new(_config.Interaction.PostAlreadyResolvedResponse);
        }

        var values = Context.SelectedUsers;

        var helper = values[0];
        if (helper.IsBot)
            throw new(_config.Interaction.SelectedBotAsHelperResponse);

        var isHelper2 = values.Count == 2;
        User? helper2;
        if (isHelper2)
        {
            helper2 = values[1];
            if (helper2.IsBot)
                throw new(_config.Interaction.SelectedBotAsHelperResponse);
        }
        else
            helper2 = null;

        var user = Context.User;
        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Content = $"**{_config.Emojis.Success} {(isHelper2 ? string.Format(_config.Interaction.WaitingForApprovalWith2HelpersResponse, helper, helper2) : string.Format(_config.Interaction.WaitingForApprovalResponse, helper))}**",
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"approve:{helper.Id}:{helper != user}:{(isHelper2 ? helper2!.Id : null)}:{(isHelper2 ? helper2 != user : null)}", _config.Interaction.ApproveButtonLabel, ButtonStyle.Success),
                }),
            },
            AllowedMentions = AllowedMentionsProperties.None,
        }));
    }
}
