﻿using Microsoft.Extensions.Configuration;

namespace ProgramowanieBot;

public class InteractionServiceContextConfig
{
    public InteractionServiceContextConfig(IConfiguration configuration)
    {
        OnlyPostCreatorResponse = configuration.GetRequiredSection("OnlyPostCreatorResponse").Value;
        OnlyPostCreatorOrModeratorResponse = configuration.GetRequiredSection("OnlyPostCreatorOrModeratorResponse").Value;
        PostCloseResponse = configuration.GetRequiredSection("PostCloseResponse").Value;
        PostCloseButton = configuration.GetRequiredSection("PostCloseButton").Value;
    }

    public string OnlyPostCreatorResponse { get; }
    public string OnlyPostCreatorOrModeratorResponse { get; }
    public string PostCloseResponse { get; }
    public string PostCloseButton { get; }
}
