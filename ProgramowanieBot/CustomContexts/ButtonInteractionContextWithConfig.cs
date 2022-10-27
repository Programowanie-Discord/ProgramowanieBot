using NetCord.Gateway;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.CustomContexts;

public class ButtonInteractionContextWithConfig : ButtonInteractionContext
{
    public InteractionServiceContextConfig Config { get; }

    public ButtonInteractionContextWithConfig(ButtonInteraction interaction, GatewayClient client, InteractionServiceContextConfig config) : base(interaction, client)
    {
        Config = config;
    }
}
