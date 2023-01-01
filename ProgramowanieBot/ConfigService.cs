using System.Text.Json;

using NetCord;

using Npgsql;

namespace ProgramowanieBot;

#nullable disable

public class ConfigService
{
    public static ConfigService Create()
    {
        var options = Serialization.Options;
        options.ReadCommentHandling = JsonCommentHandling.Skip;
        return JsonSerializer.Deserialize<ConfigService>(File.OpenRead("appsettings.json"), options)!;
    }

    public string Token { get; init; }
    public Color EmbedColor { get; init; }
    public InteractionHandlerConfig Interaction { get; init; }
    public GuildThreadHandlerConfig GuildThread { get; init; }
    public DatabaseConfig Database { get; init; }
    public DailyReputationHandlerConfig DailyReputationReactions { get; init; }
    public EmojisConfig Emojis { get; init; }
}

public class InteractionHandlerConfig
{
    public string OnlyPostCreatorResponse { get; init; }
    public string OnlyPostCreatorOrModeratorResponse { get; init; }
    public string PostClosedResponse { get; init; }
    public string PostAlreadyResolved { get; init; }
    public string PostResolvedResponse { get; init; }
    public string ShowProfileOnBotResponse { get; init; }
    public string SelectedBotAsHelperResponse { get; init; }
    public string NotHelpChannelResponse { get; init; }
}

public class GuildThreadHandlerConfig
{
    public ulong HelpChannelId { get; init; }
    public IReadOnlyDictionary<ulong, ulong> HelpTagsRoles { get; init; }
    public string HelpPostStartMessage { get; init; }
    public string MentionMenuPlaceholder { get; init; }
    public string PostCloseButtonLabel { get; init; }
}

public class DatabaseConfig
{
    public string Host { get; init; }
    public string Database { get; init; }
    public ushort? Port { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }

    public string CreateConnectionString()
    {
        NpgsqlConnectionStringBuilder builder = new();
        builder.Host = Host;
        builder.Database = Database;
        if (Port.HasValue)
            builder.Port = Port.GetValueOrDefault();
        builder.Username = Username;
        builder.Password = Password;
        return builder.ToString();
    }
}

public class DailyReputationHandlerConfig
{
    public ulong ChannelId { get; init; }
}

public class EmojisConfig
{
    public string Success { get; init; }
    public string Error { get; init; }
}
