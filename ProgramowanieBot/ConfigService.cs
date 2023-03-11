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
        using var stream = File.OpenRead("appsettings.json");
        return JsonSerializer.Deserialize<ConfigService>(stream, options)!;
    }

    public string Token { get; init; }
    public Color EmbedColor { get; init; }
    public InteractionHandlerConfig Interaction { get; init; }
    public GuildThreadHandlerConfig GuildThread { get; init; }
    public DatabaseConfig Database { get; init; }
    public DailyReputationHandlerConfig DailyReputationReactions { get; init; }
    public EmojisConfig Emojis { get; init; }

    public class InteractionHandlerConfig
    {
        public string OnlyPostCreatorResponse { get; init; }
        public string OnlyPostCreatorOrModeratorResponse { get; init; }
        public string PostClosedResponse { get; init; }
        public string PostAlreadyResolvedResponse { get; init; }
        public string PostResolvedResponse { get; init; }
        public string PostResolvedPrefix { get; init; }
        public string SyncingPostsResponse { get; init; }
        public string PostsSyncedResponse { get; init; }
        public string ShowProfileOnBotResponse { get; init; }
        public string SelectedBotAsHelperResponse { get; init; }
        public string NotHelpChannelResponse { get; init; }
        public string WaitingForApprovalResponse { get; init; }
        public string WaitingForApprovalWith2HelpersResponse { get; init; }
        public string ApproveButtonLabel { get; init; }
        public string AlreadyMentionedResponse { get; init; }
        public string NotOwnMessageResponse { get; init; }
        public string SelectHelperMenuPlaceholder { get; init; }
        public string IHelpedMyselfButtonLabel { get; init; }
        public StealEmojiConfig StealEmoji { get; init; }
        public ReactionCommandsConfig ReactionCommands { get; init; }
        public ReputationCommandsConfig ReputationCommands { get; init; }

        public class StealEmojiConfig
        {
            public string NoEmojisFoundResponse { get; init; }
            public string AddEmojiModalTitle { get; init; }
            public string AddEmojiModalNameInputLabel { get; init; }
            public string StealEmojisMenuPlaceholder { get; init; }
        }

        public class ReactionCommandsConfig
        {
            public string ReactionsAddedResponse { get; init; }
            public string ReactionsRemovedResponse { get; init; }
            public string HelpPostStartMessageResponse { get; init; }
        }

        public class ReputationCommandsConfig
        {
            public string ReputationAddedResponse { get; init; }
            public string ReputationRemovedResponse { get; init; }
            public string ReputationSetResponse { get; init; }
            public string LeaderboardEmbedTitle { get; init; }
            public string LeaderboardEmbedFooter { get; init; }
        }
    }

    public class GuildThreadHandlerConfig
    {
        public ulong HelpChannelId { get; init; }
        public IReadOnlyDictionary<ulong, ulong> HelpTagsRoles { get; init; }
        public double ReactionTypingTimeoutSeconds { get; init; }
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
        public string Loading { get; init; }
        public string Left { get; init; }
        public string Right { get; init; }
    }
}
