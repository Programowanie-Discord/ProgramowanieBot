using NetCord.Gateway;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules;

public class ExtendedUserCommandContext : UserCommandContext
{
    public ConfigService Config { get; }
    public IServiceProvider Provider { get; }

    public ExtendedUserCommandContext(UserCommandInteraction interaction, GatewayClient client, ConfigService config, IServiceProvider provider) : base(interaction, client)
    {
        Config = config;
        Provider = provider;
    }
}
