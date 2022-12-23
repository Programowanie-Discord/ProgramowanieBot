using Microsoft.Extensions.Logging;

using NetCord.Gateway;

namespace ProgramowanieBot.Handlers;

internal abstract class BaseHandler : IHandler
{
    protected BaseHandler(GatewayClient client, ILogger logger)
    {
        Client = client;
        Logger = logger;
    }

    protected GatewayClient Client { get; }
    protected ILogger Logger { get; }

    public abstract ValueTask StartAsync(CancellationToken cancellationToken);
    public abstract ValueTask StopAsync(CancellationToken cancellationToken);
}

internal abstract class BaseHandler<TConfig> : BaseHandler
{
    protected BaseHandler(GatewayClient client, ILogger logger, TConfig config) : base(client, logger)
    {
        Config = config;
    }

    protected TConfig Config { get; }
}
