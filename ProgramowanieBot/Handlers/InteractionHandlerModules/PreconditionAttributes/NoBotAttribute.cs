using NetCord;
using NetCord.Services;

using ProgramowanieBot.Handlers.InteractionHandlerModules;

namespace ProgramowanieBot;

internal class NoBotAttribute<TContext> : ParameterPreconditionAttribute<TContext> where TContext : IExtendedContext
{
    public override ValueTask EnsureCanExecuteAsync(object? value, TContext context)
    {
        if (value != null && ((User)value!).IsBot)
            throw new(context.Config.Interaction.SelectedBotAsHelperResponse);

        return default;
    }
}
