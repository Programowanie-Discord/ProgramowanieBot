using NetCord.Gateway;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.CustomContexts;

public class StringMenuInteractionContextWithConfig : StringMenuInteractionContext, IInteractionContextWithConfig
{
    public InteractionServiceConfig Config { get; }

    public StringMenuInteractionContextWithConfig(StringMenuInteraction interaction, GatewayClient client, InteractionServiceConfig config) : base(interaction, client)
    {
        Config = config;
    }
}
