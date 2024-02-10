using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.Interactions;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.Helpers;

internal static class OptionsHelper
{
    public static void ConfigureApplicationCommandService<TInteraction, TContext>(ApplicationCommandServiceOptions<TInteraction, TContext> options) where TInteraction : ApplicationCommandInteraction where TContext : IApplicationCommandContext
    {
        options.Configuration = ApplicationCommandServiceConfiguration<TContext>.Default with { DefaultDMPermission = false };
        options.HandleResultAsync = HandleResultAsync;
    }

    public static void ConfigureInteractionService<TInteraction, TContext>(InteractionServiceOptions<TInteraction, TContext> options) where TInteraction : Interaction where TContext : IInteractionContext
    {
        options.HandleResultAsync = HandleResultAsync;
    }

    private static async ValueTask HandleResultAsync<TInteraction>(IExecutionResult result, TInteraction interaction, GatewayClient? client, ILogger logger, IServiceProvider services) where TInteraction : Interaction
    {
        if (result is not IFailResult failResult)
            return;

        var configuration = services.GetRequiredService<IOptions<Configuration>>().Value;

        InteractionMessageProperties message = new()
        {
            Content = $"**{configuration.Emojis.Error} {failResult.Message}**",
            Flags = MessageFlags.Ephemeral,
        };
        try
        {
            await interaction.SendResponseAsync(InteractionCallback.Message(message));
        }
        catch (RestException restException) when (restException.StatusCode == HttpStatusCode.BadRequest)
        {
            if (restException.Error is { Code: 40060 })
                await interaction.SendFollowupMessageAsync(message);
        }
        catch
        {
        }
    }
}
