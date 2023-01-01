using NetCord.Services;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules;

internal interface IExtendedContext : IContext
{
    public ConfigService Config { get; }
    public IServiceProvider Provider { get; }
}
