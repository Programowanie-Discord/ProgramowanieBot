using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Services;

namespace ProgramowanieBot;

internal class NoBotAttribute<TContext> : ParameterPreconditionAttribute<TContext> where TContext : IContext
{
    public override ValueTask EnsureCanExecuteAsync(object? value, TContext context, IServiceProvider? serviceProvider)
    {
        if (value != null && ((User)value!).IsBot)
            throw new(serviceProvider!.GetRequiredService<ConfigService>().Interaction.SelectedBotAsHelperResponse);

        return default;
    }
}
