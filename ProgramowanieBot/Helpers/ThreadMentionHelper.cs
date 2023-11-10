using System.Text;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace ProgramowanieBot.Helpers;

internal static class ThreadMentionHelper
{
    private static readonly HashSet<ulong> _mentionedThreadIds = [];

    public static void EnsureFirstMention(ulong threadId, ConfigService config)
    {
        var mentionedThreadIds = _mentionedThreadIds;
        lock (mentionedThreadIds)
        {
            if (mentionedThreadIds.Contains(threadId))
                throw new(config.Interaction.AlreadyMentionedResponse);
            mentionedThreadIds.Add(threadId);
        }
    }

    public static async Task MentionRoleAsync(RestClient client, ulong threadId, ulong roleId, Guild guild)
    {
        var message = await client.SendMessageAsync(threadId, $"<@&{roleId}>");

        if (message.Flags.HasFlag(MessageFlags.FailedToMentionSomeRolesInThread))
        {
            StringBuilder stringBuilder = new(2000, 2000);
            List<Task> tasks = new(1);
            foreach (var user in guild.Users.Values.Where(u => u.RoleIds.Contains(roleId)))
            {
                var mention = user.ToString();
                if (stringBuilder.Length + mention.Length > 2000)
                {
                    tasks.Add(SendAndDeleteMessageAsync(stringBuilder.ToString()));
                    stringBuilder.Clear();
                }
                stringBuilder.Append(mention);
            }
            if (stringBuilder.Length != 0)
                tasks.Add(SendAndDeleteMessageAsync(stringBuilder.ToString()));

            await Task.WhenAll(tasks);

            async Task SendAndDeleteMessageAsync(string content)
            {
                var message = await client.SendMessageAsync(threadId, content);
                try
                {
                    await message.DeleteAsync();
                }
                catch
                {
                }
            }
        }
    }
}
