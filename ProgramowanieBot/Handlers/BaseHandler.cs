using Microsoft.Extensions.Logging;

using NetCord.Gateway;

namespace ProgramowanieBot.Handlers;

internal abstract class BaseHandler : IHandler
{
    protected BaseHandler(GatewayClient client, ILogger logger, IServiceProvider provider)
    {
        Client = client;
        Logger = logger;
        Provider = provider;
    }

    protected GatewayClient Client { get; }
    protected ILogger Logger { get; }
    protected IServiceProvider Provider { get; }

    public abstract ValueTask StartAsync(CancellationToken cancellationToken);
    public abstract ValueTask StopAsync(CancellationToken cancellationToken);
}

internal abstract class BaseHandler<TConfig> : BaseHandler
{
    protected BaseHandler(GatewayClient client, ILogger logger, TConfig config, IServiceProvider provider) : base(client, logger, provider)
    {
        Config = config;
    }

    protected TConfig Config { get; }
}
