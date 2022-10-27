using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.Interactions;

using ProgramowanieBot.CustomContexts;

namespace ProgramowanieBot;

internal class InteractionService : IHostedService
{
    private readonly ILogger _logger;
    private readonly ApplicationCommandService<SlashCommandContext> _applicationCommandService;
    private readonly InteractionService<ButtonInteractionContextWithConfig> _buttonInteractionService;
    private readonly InteractionService<StringMenuInteractionContextWithConfig> _stringMenuInteractionService;
    private readonly GatewayClient _client;
    private readonly ulong _clientId;
    private readonly InteractionServiceContextConfig _config;

    public InteractionService(ILogger<InteractionService> logger, BotService botService, TokenService tokenService, IConfiguration configuration)
    {
        _logger = logger;

        _applicationCommandService = new();
        _buttonInteractionService = new();
        _stringMenuInteractionService = new();

        var assembly = Assembly.GetEntryAssembly()!;
        _applicationCommandService.AddModules(assembly);
        _buttonInteractionService.AddModules(assembly);
        _stringMenuInteractionService.AddModules(assembly);

        _client = botService.Client;
        _client.InteractionCreate += HandleInteractionAsync;
        _clientId = tokenService.Token.Id;
        _config = new(configuration);
    }

    private async ValueTask HandleInteractionAsync(Interaction interaction)
    {
        try
        {
            await (interaction switch
            {
                SlashCommandInteraction slashCommandInteraction => _applicationCommandService.ExecuteAsync(new(slashCommandInteraction, _client)),
                ButtonInteraction buttonInteraction => _buttonInteractionService.ExecuteAsync(new(buttonInteraction, _client, _config)),
                StringMenuInteraction stringMenuInteraction => _stringMenuInteractionService.ExecuteAsync(new(stringMenuInteraction, _client, _config)),
                _ => throw new("Invalid interaction."),
            });
        }
        catch (Exception ex)
        {
            InteractionMessageProperties message = new()
            {
                Content = $"<a:nie:881595378070343710> {ex.Message}",
                Flags = MessageFlags.Ephemeral,
            };
            try
            {
                await interaction.SendResponseAsync(InteractionCallback.ChannelMessageWithSource(message));
            }
            catch (RestException restException) when (restException.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                if (await restException.GetDiscordStatusCodeAsync() == 40060)
                    await interaction.SendFollowupMessageAsync(message);
            }
            catch
            {
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering application commands");
        var list = await _applicationCommandService.CreateCommandsAsync(_client.Rest, _clientId);
        _logger.LogInformation("{count} command(s) successfully registered", list.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.InteractionCreate -= HandleInteractionAsync;
        return Task.CompletedTask;
    }
}
