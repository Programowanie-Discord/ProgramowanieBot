using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.Interactions;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.Interactions;

namespace ProgramowanieBot.Helpers;

internal static class OptionsHelper
{
    public static void ConfigureApplicationCommandService<TInteraction, TContext>(ApplicationCommandServiceOptions<TInteraction, TContext> options) where TInteraction : ApplicationCommandInteraction where TContext : IApplicationCommandContext
    {
        options.Configuration = ApplicationCommandServiceConfiguration<TContext>.Default with { DefaultDMPermission = false };
        options.HandleExceptionAsync = HandleExceptionAsync;
    }

    public static void ConfigureInteractionService<TInteraction, TContext>(InteractionServiceOptions<TInteraction, TContext> options) where TInteraction : Interaction where TContext : IInteractionContext
    {
        options.HandleExceptionAsync = HandleExceptionAsync;
    }

    private static async ValueTask HandleExceptionAsync<TInteraction>(Exception exception, TInteraction interaction, GatewayClient? client, ILogger logger, IServiceProvider services) where TInteraction : Interaction
    {
        var configuration = services.GetRequiredService<Configuration>();

        InteractionMessageProperties message = new()
        {
            Content = $"**{configuration.Emojis.Error} {exception.Message}**",
            Flags = MessageFlags.Ephemeral,
        };
        try
        {
            await interaction.SendResponseAsync(InteractionCallback.Message(message));
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
