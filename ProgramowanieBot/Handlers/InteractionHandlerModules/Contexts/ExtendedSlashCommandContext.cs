using NetCord.Gateway;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules;

public class ExtendedSlashCommandContext : SlashCommandContext, IExtendedContext
{
    public ConfigService Config { get; }
    public IServiceProvider Provider { get; }

    public ExtendedSlashCommandContext(SlashCommandInteraction interaction, GatewayClient client, ConfigService config, IServiceProvider provider) : base(interaction, client)
    {
        Provider = provider;
        Config = config;
    }
}
