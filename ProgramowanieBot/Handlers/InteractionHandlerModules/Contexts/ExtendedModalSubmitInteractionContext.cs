using NetCord.Gateway;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules;

public class ExtendedModalSubmitInteractionContext : ModalSubmitInteractionContext, IExtendedContext
{
    public ConfigService Config { get; }
    public IServiceProvider Provider { get; }

    public ExtendedModalSubmitInteractionContext(ModalSubmitInteraction interaction, GatewayClient client, ConfigService config, IServiceProvider provider) : base(interaction, client)
    {
        Config = config;
        Provider = provider;
    }
}
