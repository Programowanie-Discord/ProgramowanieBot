using Microsoft.Extensions.Configuration;

namespace ProgramowanieBot;

public class InteractionServiceContextConfig
{
    public InteractionServiceContextConfig(IConfiguration configuration)
    {
        OnlyPostCreatorResponse = configuration.GetRequiredSection("OnlyPostCreatorResponse").Value;
    }

    public string OnlyPostCreatorResponse { get; }
}
