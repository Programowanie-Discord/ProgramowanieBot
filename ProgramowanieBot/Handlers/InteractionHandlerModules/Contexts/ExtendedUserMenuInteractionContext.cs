using NetCord.Gateway;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules;

public class ExtendedUserMenuInteractionContext : UserMenuInteractionContext, IExtendedContext
{
    public ConfigService Config { get; }
    public IServiceProvider Provider { get; }

    public ExtendedUserMenuInteractionContext(UserMenuInteraction interaction, GatewayClient client, ConfigService config, IServiceProvider provider) : base(interaction, client)
    {
        Config = config;
        Provider = provider;
    }
}
