using System.Text.Json;

using NetCord;

namespace ProgramowanieBot;

#nullable disable

internal class ConfigService
{
    public static ConfigService Create()
    {
        var options = Serialization.Options;
        options.ReadCommentHandling = JsonCommentHandling.Skip;
        return JsonSerializer.Deserialize<ConfigService>(File.OpenRead("appsettings.json"), options)!;
    }

    public string Token { get; init; }
    public InteractionServiceConfig Interaction { get; init; }
    public GuildThreadServiceConfig GuildThread { get; init; }
}

public class InteractionServiceConfig
{
    public string OnlyPostCreatorResponse { get; init; }
    public string OnlyPostCreatorOrModeratorResponse { get; init; }
    public string PostCloseResponse { get; init; }
    public string PostCloseButtonLabel { get; init; }
}

public class GuildThreadServiceConfig
{
    public ulong ForumChannelId { get; init; }
    public IReadOnlyDictionary<ulong, ulong> ForumTagsRoles { get; init; }
    public string ForumPostStartMessage { get; init; }
    public string MentionMenuPlaceholder { get; init; }
    public string PostCloseButtonLabel { get; init; }
}
