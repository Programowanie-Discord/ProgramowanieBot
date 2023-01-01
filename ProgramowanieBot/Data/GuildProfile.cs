using System.ComponentModel.DataAnnotations;

namespace ProgramowanieBot.Data;

public class GuildProfile
{
    [Key]
    public ulong UserId { get; set; }

    public long Reputation { get; set; }

    public long ReputationToday { get; set; }
}
