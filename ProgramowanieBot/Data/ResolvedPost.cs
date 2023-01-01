using System.ComponentModel.DataAnnotations;

namespace ProgramowanieBot.Data;

public class ResolvedPost
{
    [Key]
    public ulong Id { get; set; }
}
