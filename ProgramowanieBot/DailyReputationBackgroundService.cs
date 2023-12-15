using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetCord.Gateway;
using NetCord.Rest;

using ProgramowanieBot.Data;

namespace ProgramowanieBot;

internal class DailyReputationBackgroundService(GatewayClient client, Configuration configuration, IServiceProvider services) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var day = TimeSpan.FromDays(1);
        await Task.Delay(day - DateTimeOffset.UtcNow.TimeOfDay + TimeSpan.FromHours(5), stoppingToken);
        using PeriodicTimer timer = new(day);
        while (true)
        {
            await using (var context = services.GetRequiredService<DataContext>())
            {
                await using var transaction = await context.Database.BeginTransactionAsync(default);
                var profiles = context.Profiles.Where(p => p.ReputationToday != 0L).OrderByDescending(p => Math.Abs(p.ReputationToday));
                StringBuilder stringBuilder = new(4096, 4096);
                foreach (var profile in profiles)
                {
                    var line = $"<@{profile.UserId}>: {profile.ReputationToday:+#;-#}";
                    profile.ReputationToday = 0;
                    if (stringBuilder.Length + line.Length >= 4096)
                    {
                        await client.Rest.SendMessageAsync(configuration.DailyReputationReactions.ChannelId, new()
                        {
                            Embeds = new EmbedProperties[]
                            {
                                new()
                                {
                                    Description = stringBuilder.ToString(),
                                    Color = configuration.EmbedColor,
                                },
                            },
                        });
                        stringBuilder.Clear();
                    }
                    stringBuilder.AppendLine(line);
                }
                if (stringBuilder.Length != 0)
                    await client.Rest.SendMessageAsync(configuration.DailyReputationReactions.ChannelId, new()
                    {
                        Embeds = new EmbedProperties[]
                        {
                            new()
                            {
                                Description = stringBuilder.ToString(),
                                Color = configuration.EmbedColor,
                            },
                        },
                    });
                await context.SaveChangesAsync(default);
                await transaction.CommitAsync(default);
            }
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }
}
