using System.Reflection;

using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.Interactions;

using ProgramowanieBot.Handlers.InteractionHandlerModules;
using ProgramowanieBot.Helpers;

namespace ProgramowanieBot.Handlers;

internal class InteractionHandler : BaseHandler<ConfigService>
{
    private readonly ApplicationCommandServiceManager _applicationCommandServiceManager;

    private readonly ApplicationCommandService<ExtendedSlashCommandContext> _slashCommandService;
    private readonly ApplicationCommandService<ExtendedUserCommandContext> _userCommandService;
    private readonly ApplicationCommandService<ExtendedMessageCommandContext> _messageCommandService;

    private readonly InteractionService<ExtendedButtonInteractionContext> _buttonInteractionService;
    private readonly InteractionService<ExtendedStringMenuInteractionContext> _stringMenuInteractionService;
    private readonly InteractionService<ExtendedUserMenuInteractionContext> _userMenuInteractionService;
    private readonly InteractionService<ExtendedModalSubmitInteractionContext> _modalSubmitInteractionService;

    private readonly TokenService _token;

    public InteractionHandler(GatewayClient client, ILogger<InteractionHandler> logger, ConfigService config, TokenService token, IServiceProvider provider) : base(client, logger, config, provider)
    {
        _applicationCommandServiceManager = new();
        _slashCommandService = new(new()
        {
            DefaultDMPermission = false,
        });
        _userCommandService = new(new()
        {
            DefaultDMPermission = false,
        });
        _messageCommandService = new(new()
        {
            DefaultDMPermission = false,
        });
        _buttonInteractionService = new();
        _stringMenuInteractionService = new();
        _userMenuInteractionService = new();
        _modalSubmitInteractionService = new();
        _token = token;
    }

    public override async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        _applicationCommandServiceManager.AddService(_slashCommandService);
        _applicationCommandServiceManager.AddService(_userCommandService);
        _applicationCommandServiceManager.AddService(_messageCommandService);

        var assembly = Assembly.GetEntryAssembly()!;
        _slashCommandService.AddModules(assembly);
        _userCommandService.AddModules(assembly);
        _messageCommandService.AddModules(assembly);

        _buttonInteractionService.AddModules(assembly);
        _stringMenuInteractionService.AddModules(assembly);
        _userMenuInteractionService.AddModules(assembly);
        _modalSubmitInteractionService.AddModules(assembly);

        Logger.LogInformation("Registering application commands");
        var list = await _applicationCommandServiceManager.CreateCommandsAsync(Client.Rest, _token.Token.Id);
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
                SlashCommandInteraction slashCommandInteraction => _slashCommandService.ExecuteAsync(new(slashCommandInteraction, Client, Config, Provider)),
                UserCommandInteraction userCommandInteraction => _userCommandService.ExecuteAsync(new(userCommandInteraction, Client, Config, Provider)),
                MessageCommandInteraction messageCommandInteraction => _messageCommandService.ExecuteAsync(new(messageCommandInteraction, Client, Config, Provider)),
                ButtonInteraction buttonInteraction => _buttonInteractionService.ExecuteAsync(new(buttonInteraction, Client, Config, Provider)),
                StringMenuInteraction stringMenuInteraction => _stringMenuInteractionService.ExecuteAsync(new(stringMenuInteraction, Client, Config, Provider)),
                UserMenuInteraction userMenuInteraction => _userMenuInteractionService.ExecuteAsync(new(userMenuInteraction, Client, Config, Provider)),
                ModalSubmitInteraction modalSubmitInteraction => _modalSubmitInteractionService.ExecuteAsync(new(modalSubmitInteraction, Client, Config, Provider)),
                _ => throw new("Invalid interaction."),
            });
        }
        catch (Exception ex)
        {
            InteractionMessageProperties message = new()
            {
                Content = $"**{Config.Emojis.Error} {ex.Message}**",
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
