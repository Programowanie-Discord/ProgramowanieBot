using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;
using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Interactions.ButtonInteractions;

public class MentionInteraction(ConfigService config, IServiceProvider serviceProvider) : InteractionModule<ButtonInteractionContext>
{
    [Interaction("mention")]
    public async Task MentionAsync([AllowedUser<ButtonInteractionContext>] ulong threadOwnerId, ulong roleId)
    {
        var channelId = Context.Channel.Id;

        bool resolved;
        await using (var context = serviceProvider.GetRequiredService<DataContext>())
            resolved = await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved);

        if (resolved)
            throw new(config.Interaction.PostAlreadyResolvedResponse);

        ThreadMentionHelper.EnsureFirstMention(channelId, config);

        await RespondAsync(InteractionCallback.ModifyMessage(m =>
        {
            m.Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"close:{threadOwnerId}", config.GuildThread.PostCloseButtonLabel, ButtonStyle.Danger),
                }),
            };
        }));
        await ThreadMentionHelper.MentionRoleAsync(Context.Client.Rest, channelId, roleId, Context.Guild!);
    }
}
