using System.Reflection;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot;

internal class ApplicationCommandService : IHostedService
{
    private readonly ILogger _logger;
    private readonly ApplicationCommandService<SlashCommandContext> _applicationCommandService;
    private readonly GatewayClient _client;
    private readonly Snowflake _clientId;

    public ApplicationCommandService(ILogger<ApplicationCommandService> logger, BotService botService, TokenService tokenService)
    {
        _logger = logger;
        _applicationCommandService = new();
        _applicationCommandService.AddModules(Assembly.GetEntryAssembly()!);
        _client = botService.Client;
        _client.InteractionCreate += HandleInteractionAsync;
        _clientId = tokenService.Token.Id;
    }

    private async ValueTask HandleInteractionAsync(Interaction interaction)
    {
        try
        {
            await (interaction switch
            {
                SlashCommandInteraction slashCommandInteraction => _applicationCommandService.ExecuteAsync(new(slashCommandInteraction, _client)),
                _ => throw new("Invalid interaction."),
            });
        }
        catch (Exception ex)
        {
            try
            {
                await interaction.SendResponseAsync(InteractionCallback.ChannelMessageWithSource(new()
                {
                    Content = $"<a:nie:881595378070343710> {ex.Message}",
                    Flags = MessageFlags.Ephemeral,
                }));
            }
            catch
            {
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _client.StartAsync();
        _logger.LogInformation("Registering application commands");
        var list = await _applicationCommandService.CreateCommandsAsync(_client.Rest, _clientId);
        _logger.LogInformation("{count} command(s) successfully registered", list.Count);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.CloseAsync();
        _client.InteractionCreate -= HandleInteractionAsync;
    }
}
