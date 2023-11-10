using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.UserCommands;

public class ShowProfileCommand(IServiceProvider serviceProvider, ConfigService config) : ApplicationCommandModule<UserCommandContext>
{
    [UserCommand("Show Profile", NameTranslationsProviderType = typeof(NameTranslationsProvider))]
    public async Task<InteractionCallback> ShowProfileAsync()
    {
        await using var context = serviceProvider.GetRequiredService<DataContext>();
        var target = Context.Target;
        if (target.IsBot)
            throw new(config.Interaction.ShowProfileOnBotResponse);

        var user = Context.User;
        var profile = await context.Profiles.FirstOrDefaultAsync(u => u.UserId == target.Id);
        long reputation = profile != null ? profile.Reputation : 0L;
        return InteractionCallback.Message(new()
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
                            Name = "Reputation",
                            Value = reputation.ToString(),
                        },
                    },
                    Footer = new()
                    {
                        IconUrl = user.HasAvatar ? user.GetAvatarUrl().ToString() : user.DefaultAvatarUrl.ToString(),
                        Text = $"{user.Username}#{user.Discriminator:D4}",
                    },
                    Color = config.EmbedColor,
                }
            },
            Flags = MessageFlags.Ephemeral,
        });
    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Pokaż profil" },
        };
    }
}
