using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetCord.Gateway;

using ProgramowanieBot.Handlers;

namespace ProgramowanieBot;

internal class BotService : IHostedService
{
    private readonly GatewayClient _client;

    private readonly ILogger _logger;
    private readonly IEnumerable<IHandler> _handlers;

    public BotService(ILogger<BotService> logger, GatewayClient client, IEnumerable<IHandler> handlers)
    {
        _logger = logger;
        _client = client;
        _client.Log += message =>
        {
            _logger.Log(message.Severity switch
            {
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Error => LogLevel.Error,
                _ => LogLevel.Warning
            }, "{message} {description}", message.Message, message.Description ?? string.Empty);
            return default;
        };
        _handlers = handlers;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting handlers");
        foreach (var handler in _handlers)
            await handler.StartAsync(cancellationToken);
        _logger.LogInformation("Handlers started");
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.CloseAsync();
        _logger.LogInformation("Stopping handlers");
        foreach (var handler in _handlers)
            await handler.StopAsync(cancellationToken);
        _logger.LogInformation("Handlers stopped");
    }
}
