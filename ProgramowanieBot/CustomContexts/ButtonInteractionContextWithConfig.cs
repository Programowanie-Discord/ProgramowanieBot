using NetCord.Gateway;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.CustomContexts;

public class ButtonInteractionContextWithConfig : ButtonInteractionContext, IInteractionContextWithConfig
{
    public InteractionServiceConfig Config { get; }

    public ButtonInteractionContextWithConfig(ButtonInteraction interaction, GatewayClient client, InteractionServiceConfig config) : base(interaction, client)
    {
        Config = config;
    }
}
