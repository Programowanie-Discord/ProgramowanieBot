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
    private readonly ApplicationCommandService<ExtendedSlashCommandContext> _applicationCommandService;
    private readonly ApplicationCommandService<ExtendedUserCommandContext> _userCommandService;
    private readonly InteractionService<ExtendedButtonInteractionContext> _buttonInteractionService;
    private readonly InteractionService<ExtendedStringMenuInteractionContext> _stringMenuInteractionService;

    private readonly TokenService _token;

    public InteractionHandler(GatewayClient client, ILogger<InteractionHandler> logger, ConfigService config, TokenService token, IServiceProvider provider) : base(client, logger, config, provider)
    {
        _applicationCommandServiceManager = new();
        _applicationCommandService = new(new()
        {
            DefaultDMPermission = false,
        });
        _userCommandService = new(new()
        {
            DefaultDMPermission = false,
        });
        _buttonInteractionService = new();
        _stringMenuInteractionService = new();
        _token = token;
    }

    public override async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        _applicationCommandServiceManager.AddService(_applicationCommandService);
        _applicationCommandServiceManager.AddService(_userCommandService);

        var assembly = Assembly.GetEntryAssembly()!;
        _applicationCommandService.AddModules(assembly);
        _userCommandService.AddModules(assembly);
        _buttonInteractionService.AddModules(assembly);
        _stringMenuInteractionService.AddModules(assembly);

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
                SlashCommandInteraction slashCommandInteraction => _applicationCommandService.ExecuteAsync(new(slashCommandInteraction, Client, Config, Provider)),
                UserCommandInteraction userCommandInteraction => _userCommandService.ExecuteAsync(new(userCommandInteraction, Client, Config, Provider)),
                ButtonInteraction buttonInteraction => _buttonInteractionService.ExecuteAsync(new(buttonInteraction, Client, Config, Provider)),
                StringMenuInteraction stringMenuInteraction => _stringMenuInteractionService.ExecuteAsync(new(stringMenuInteraction, Client, Config, Provider)),
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
