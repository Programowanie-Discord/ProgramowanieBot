using System.Text.Json;

using NetCord;

using Npgsql;

namespace ProgramowanieBot;

#nullable disable

public class Configuration
{
    public static Configuration Create()
    {
        var options = Serialization.Options;
        options.ReadCommentHandling = JsonCommentHandling.Skip;
        using var stream = File.OpenRead("appsettings.json");
        return JsonSerializer.Deserialize<Configuration>(stream, options)!;
    }

    public DailyReputationHandlerConfiguration DailyReputationReactions { get; init; }
    public DatabaseConfiguration Database { get; init; }
    public Color EmbedColor { get; init; }
    public EmojisConfiguration Emojis { get; init; }
    public GuildThreadHandlerConfiguration GuildThread { get; init; }
    public InteractionHandlerConfiguration Interaction { get; init; }

    public class DailyReputationHandlerConfiguration
    {
        public ulong ChannelId { get; init; }
    }

    public class DatabaseConfiguration
    {
        public string Database { get; init; }
        public string Host { get; init; }
        public string Password { get; init; }
        public ushort? Port { get; init; }
        public string Username { get; init; }

        public string CreateConnectionString()
        {
            NpgsqlConnectionStringBuilder builder = new()
            {
                Database = Database,
                Host = Host,
                Password = Password,
                Username = Username
            };
            if (Port.HasValue)
                builder.Port = Port.GetValueOrDefault();

            return builder.ToString();
        }
    }

    public class EmojisConfiguration
    {
        public string Error { get; init; }
        public string Left { get; init; }
        public string Loading { get; init; }
        public string Right { get; init; }
        public string Success { get; init; }
    }

    public class GuildThreadHandlerConfiguration
    {
        public ulong HelpChannelId { get; init; }
        public string HelpPostStartMessage { get; init; }
        public IReadOnlyDictionary<ulong, ulong> HelpTagsRoles { get; init; }
        public int MaxPostResolveReminders { get; init; }
        public string MentionMenuPlaceholder { get; init; }
        public string PostCloseButtonLabel { get; init; }
        public IReadOnlyList<string> PostResolveReminderKeywords { get; init; }
        public string PostResolveReminderMessage { get; init; }
        public double ReactionTypingTimeoutSeconds { get; init; }
    }

    public class InteractionHandlerConfiguration
    {
        public string AlreadyMentionedResponse { get; init; }
        public string ApproveButtonLabel { get; init; }
        public string IHelpedMyselfButtonLabel { get; init; }
        public string NotHelpChannelResponse { get; init; }
        public string NotOwnMessageResponse { get; init; }
        public string OnlyPostCreatorOrModeratorResponse { get; init; }
        public string OnlyPostCreatorResponse { get; init; }
        public string PostAlreadyResolvedResponse { get; init; }
        public string PostClosedResponse { get; init; }
        public string PostResolvedPrefix { get; init; }
        public string PostResolvedResponse { get; init; }
        public ulong PostResolvedNotificationChannelId { get; init; }
        public string PostResolvedNotificationMessage { get; init; }
        public string PostsSyncedResponse { get; init; }
        public ReactionCommandsConfiguration ReactionCommands { get; init; }
        public ReputationCommandsConfiguration ReputationCommands { get; init; }
        public string SelectedBotAsHelperResponse { get; init; }
        public string SelectHelperMenuPlaceholder { get; init; }
        public string ShowProfileOnBotResponse { get; init; }
        public StealEmojiConfiguration StealEmoji { get; init; }
        public string SyncingPostsResponse { get; init; }
        public string WaitingForApprovalResponse { get; init; }
        public string WaitingForApprovalWith2HelpersResponse { get; init; }

        public class ReactionCommandsConfiguration
        {
            public string HelpPostStartMessageResponse { get; init; }
            public string ReactionsAddedResponse { get; init; }
            public string ReactionsRemovedResponse { get; init; }
        }

        public class ReputationCommandsConfiguration
        {
            public string LeaderboardEmbedFooter { get; init; }
            public string LeaderboardEmbedTitle { get; init; }
            public string ReputationAddedResponse { get; init; }
            public string ReputationRemovedResponse { get; init; }
            public string ReputationSetResponse { get; init; }
        }

        public class StealEmojiConfiguration
        {
            public string AddEmojiModalNameInputLabel { get; init; }
            public string AddEmojiModalTitle { get; init; }
            public string NoEmojisFoundResponse { get; init; }
            public string StealEmojisMenuPlaceholder { get; init; }
        }
    }
}
