using System.ComponentModel.DataAnnotations;

namespace ProgramowanieBot.Data;

public class Post
{
    [Key]
    public ulong PostId { get; set; }
    public short PostResolveReminderCounter { get; set; }
    public bool IsResolved { get; set; }
}
