using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.InteractionHandlerModules.Interactions.ButtonInteractions;

public class MentionInteraction(IOptions<Configuration> options, IServiceProvider serviceProvider) : InteractionModule<ButtonInteractionContext>
{
    [Interaction("mention")]
    public async Task MentionAsync([AllowedUser<ButtonInteractionContext>] ulong threadOwnerId, ulong roleId)
    {
        var configuration = options.Value;

        var channelId = Context.Channel.Id;

        bool resolved;
        await using (var context = serviceProvider.GetRequiredService<DataContext>())
            resolved = await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved);

        if (resolved)
            throw new(configuration.Interaction.PostAlreadyResolvedResponse);

        ThreadMentionHelper.EnsureFirstMention(channelId, options);

        await RespondAsync(InteractionCallback.ModifyMessage(m =>
        {
            m.Components =
            [
                new ActionRowProperties(
                [
                    new ActionButtonProperties($"close:{threadOwnerId}", configuration.GuildThread.PostCloseButtonLabel, ButtonStyle.Danger),
                ]),
            ];
        }));
        await ThreadMentionHelper.MentionRoleAsync(Context.Client.Rest, channelId, roleId, Context.Guild!);
    }
}
