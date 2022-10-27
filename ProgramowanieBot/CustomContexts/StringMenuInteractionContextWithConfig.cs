using NetCord.Gateway;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.CustomContexts;

public class StringMenuInteractionContextWithConfig : StringMenuInteractionContext
{
    public InteractionServiceContextConfig Config { get; }

    public StringMenuInteractionContextWithConfig(StringMenuInteraction interaction, GatewayClient client, InteractionServiceContextConfig config) : base(interaction, client)
    {
        Config = config;
    }
}
