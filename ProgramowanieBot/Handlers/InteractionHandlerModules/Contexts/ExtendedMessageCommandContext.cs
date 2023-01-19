using NetCord.Gateway;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules;

public class ExtendedMessageCommandContext : MessageCommandContext, IExtendedContext
{
    public ConfigService Config { get; }
    public IServiceProvider Provider { get; }

    public ExtendedMessageCommandContext(MessageCommandInteraction interaction, GatewayClient client, ConfigService config, IServiceProvider provider) : base(interaction, client)
    {
        Config = config;
        Provider = provider;
    }
}
