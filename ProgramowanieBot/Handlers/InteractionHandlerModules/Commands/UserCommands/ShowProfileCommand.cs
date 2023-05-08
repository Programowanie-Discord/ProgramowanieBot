using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.UserCommands;

public class ShowProfileCommand : ApplicationCommandModule<UserCommandContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigService _config;

    public ShowProfileCommand(IServiceProvider serviceProvider, ConfigService config)
    {
        _serviceProvider = serviceProvider;
        _config = config;
    }

    [UserCommand("Show Profile", NameTranslationsProviderType = typeof(NameTranslationsProvider))]
    public async Task ShowProfileAsync()
    {
        await using var context = _serviceProvider.GetRequiredService<DataContext>();
        var target = Context.Target;
        if (target.IsBot)
            throw new(_config.Interaction.ShowProfileOnBotResponse);

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
                    Color = _config.EmbedColor,
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
