using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.UserCommands;

public class ShowProfileCommand : ApplicationCommandModule<ExtendedUserCommandContext>
{
    [UserCommand("Show Profile", NameTranslationsProviderType = typeof(NameTranslationsProvider))]
    public async Task ShowProfileAsync()
    {
        await using var context = Context.Provider.GetRequiredService<DataContext>();
        var target = Context.Target;
        if (target.IsBot)
            throw new(Context.Config.Interaction.ShowProfileOnBotResponse);

        var user = Context.User;
        var profile = await context.Profiles.FirstOrDefaultAsync(u => u.UserId == target.Id);
        long reputation = profile != null ? profile.Reputation : 0L;
        await RespondAsync(InteractionCallback.ChannelMessageWithSource(new()
        {
            Embeds = new EmbedProperties[]
            {
                new()
                {
                    Author = new()
                    {
                        Name = $"{target.Username}#{target.Discriminator:D4}",
                        IconUrl = target.HasAvatar ? target.GetAvatarUrl().ToString() : target.DefaultAvatarUrl.ToString(),
                    },
                    Fields = new EmbedFieldProperties[]
                    {
                        new()
                        {
                            Title = "Reputation",
                            Description = reputation.ToString(),
                        },
                    },
                    Footer = new()
                    {
                        IconUrl = user.HasAvatar ? user.GetAvatarUrl().ToString() : user.DefaultAvatarUrl.ToString(),
                        Text = $"{user.Username}#{user.Discriminator:D4}",
                    },
                    Color = Context.Config.EmbedColor,
                }
            },
            Flags = MessageFlags.Ephemeral,
        }));
    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Pokaż profil" },
        };
    }
}
