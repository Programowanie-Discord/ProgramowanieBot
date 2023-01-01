using Microsoft.EntityFrameworkCore;

namespace ProgramowanieBot.Data;

internal class DataContext : DbContext
{
    public DbSet<GuildProfile> Profiles { get; set; }

    public DbSet<ResolvedPost> ResolvedPosts { get; set; }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
}
