using NetCord.Services;

namespace ProgramowanieBot.CustomContexts;

internal interface IInteractionContextWithConfig : IContext
{
    public InteractionServiceConfig Config { get; }
}
