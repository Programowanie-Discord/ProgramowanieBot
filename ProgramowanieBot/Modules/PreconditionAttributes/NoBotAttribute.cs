using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Services;

namespace ProgramowanieBot.InteractionHandlerModules;

internal class NoBotAttribute<TContext> : ParameterPreconditionAttribute<TContext>
{
    public override ValueTask<PreconditionResult> EnsureCanExecuteAsync(object? value, TContext context, IServiceProvider? serviceProvider)
    {
        if (value != null && ((User)value!).IsBot)
            return new(PreconditionResult.Fail(serviceProvider!.GetRequiredService<IOptions<Configuration>>().Value.Interaction.SelectedBotAsHelperResponse));

        return new(PreconditionResult.Success);
    }
}
