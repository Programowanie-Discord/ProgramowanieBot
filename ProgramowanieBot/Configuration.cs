using Npgsql;

namespace ProgramowanieBot;

#nullable disable

public class Configuration
{
    public DailyReputationHandlerConfiguration DailyReputationReactions { get; set; }
    public DatabaseConfiguration Database { get; set; }
    public int EmbedColor { get; set; }
    public EmojisConfiguration Emojis { get; set; }
    public GuildThreadHandlerConfiguration GuildThread { get; set; }
    public InteractionHandlerConfiguration Interaction { get; set; }

    public class DailyReputationHandlerConfiguration
    {
        public ulong ChannelId { get; set; }
    }

    public class DatabaseConfiguration
    {
        public string Database { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public ushort? Port { get; set; }
        public string Username { get; set; }

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
        public string Error { get; set; }
        public string Left { get; set; }
        public string Loading { get; set; }
        public string Right { get; set; }
        public string Success { get; set; }
    }

    public class GuildThreadHandlerConfiguration
    {
        public ulong HelpChannelId { get; set; }
        public string HelpPostStartMessage { get; set; }
        public IReadOnlyDictionary<ulong, ulong> HelpTagsRoles { get; set; }
        public int MaxPostResolveReminders { get; set; }
        public string MentionMenuPlaceholder { get; set; }
        public string PostCloseButtonLabel { get; set; }
        public IReadOnlyList<string> PostResolveReminderKeywords { get; set; }
        public string PostResolveReminderMessage { get; set; }
        public double ReactionTypingTimeoutSeconds { get; set; }
    }

    public class InteractionHandlerConfiguration
    {
        public string AlreadyMentionedResponse { get; set; }
        public string ApproveButtonLabel { get; set; }
        public string IHelpedMyselfButtonLabel { get; set; }
        public string NotHelpChannelResponse { get; set; }
        public string NotOwnMessageResponse { get; set; }
        public string OnlyPostCreatorOrModeratorResponse { get; set; }
        public string OnlyPostCreatorResponse { get; set; }
        public string PostAlreadyResolvedResponse { get; set; }
        public string PostClosedResponse { get; set; }
        public string PostResolvedPrefix { get; set; }
        public string PostResolvedResponse { get; set; }
        public ulong PostResolvedNotificationChannelId { get; set; }
        public string PostResolvedNotificationMessage { get; set; }
        public string PostsSyncedResponse { get; set; }
        public ReactionCommandsConfiguration ReactionCommands { get; set; }
        public ReputationCommandsConfiguration ReputationCommands { get; set; }
        public string SelectedBotAsHelperResponse { get; set; }
        public string SelectHelperMenuPlaceholder { get; set; }
        public string ShowProfileOnBotResponse { get; set; }
        public StealEmojiConfiguration StealEmoji { get; set; }
        public string SyncingPostsResponse { get; set; }
        public string WaitingForApprovalResponse { get; set; }
        public string WaitingForApprovalWith2HelpersResponse { get; set; }

        public class ReactionCommandsConfiguration
        {
            public string HelpPostStartMessageResponse { get; set; }
            public string ReactionsAddedResponse { get; set; }
            public string ReactionsRemovedResponse { get; set; }
        }

        public class ReputationCommandsConfiguration
        {
            public string LeaderboardEmbedFooter { get; set; }
            public string LeaderboardEmbedTitle { get; set; }
            public string ReputationAddedResponse { get; set; }
            public string ReputationRemovedResponse { get; set; }
            public string ReputationSetResponse { get; set; }
        }

        public class StealEmojiConfiguration
        {
            public string AddEmojiModalNameInputLabel { get; set; }
            public string AddEmojiModalTitle { get; set; }
            public string NoEmojisFoundResponse { get; set; }
            public string StealEmojisMenuPlaceholder { get; set; }
        }
    }
}
