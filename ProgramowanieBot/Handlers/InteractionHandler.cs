using System.Reflection;

using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.Interactions;

using ProgramowanieBot.CustomContexts;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers;

internal class InteractionHandler : BaseHandler<InteractionServiceConfig>
{
    private readonly ApplicationCommandService<SlashCommandContext> _applicationCommandService;
    private readonly InteractionService<ButtonInteractionContextWithConfig> _buttonInteractionService;
    private readonly InteractionService<StringMenuInteractionContextWithConfig> _stringMenuInteractionService;

    private readonly TokenService _token;

    public InteractionHandler(GatewayClient client, ILogger<InteractionHandler> logger, ConfigService config, TokenService token) : base(client, logger, config.Interaction)
    {
        _applicationCommandService = new();
        _buttonInteractionService = new();
        _stringMenuInteractionService = new();
        _token = token;
    }

    public override async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetEntryAssembly()!;
        _applicationCommandService.AddModules(assembly);
        _buttonInteractionService.AddModules(assembly);
        _stringMenuInteractionService.AddModules(assembly);
        Logger.LogInformation("Registering application commands");
        var list = await _applicationCommandService.CreateCommandsAsync(Client.Rest, _token.Token.Id);
        Logger.LogInformation("{count} command(s) successfully registered", list.Count);
        Client.InteractionCreate += HandleInteractionAsync;
    }

    public override ValueTask StopAsync(CancellationToken cancellationToken)
    {
        Client.InteractionCreate -= HandleInteractionAsync;
        return default;
    }

    private async ValueTask HandleInteractionAsync(Interaction interaction)
    {
        try
        {
            await (interaction switch
            {
                SlashCommandInteraction slashCommandInteraction => _applicationCommandService.ExecuteAsync(new(slashCommandInteraction, Client)),
                ButtonInteraction buttonInteraction => _buttonInteractionService.ExecuteAsync(new(buttonInteraction, Client, Config)),
                StringMenuInteraction stringMenuInteraction => _stringMenuInteractionService.ExecuteAsync(new(stringMenuInteraction, Client, Config)),
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
}
