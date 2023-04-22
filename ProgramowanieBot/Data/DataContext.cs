using Microsoft.EntityFrameworkCore;

namespace ProgramowanieBot.Data;

internal class DataContext : DbContext
{
    public DbSet<GuildProfile> Profiles { get; set; }

    public DbSet<Post> Posts { get; set; }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
}
