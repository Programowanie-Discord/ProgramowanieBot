using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Services;

namespace ProgramowanieBot.InteractionHandlerModules;

internal class NoBotAttribute<TContext> : ParameterPreconditionAttribute<TContext>
{
    public override ValueTask EnsureCanExecuteAsync(object? value, TContext context, IServiceProvider? serviceProvider)
    {
        if (value != null && ((User)value!).IsBot)
            throw new(serviceProvider!.GetRequiredService<Configuration>().Interaction.SelectedBotAsHelperResponse);

        return default;
    }
}
