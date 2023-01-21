using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord.Gateway;
using NetCord.Rest;

using ProgramowanieBot.Data;

namespace ProgramowanieBot.Handlers;

internal class DailyReputationHandler : BaseHandler<ConfigService>
{
    private readonly CancellationTokenSource _tokenSource;
    private Task? _runAsync;

    public DailyReputationHandler(GatewayClient client, ILogger<DailyReputationHandler> logger, ConfigService config, IServiceProvider provider) : base(client, logger, config, provider)
    {
        _tokenSource = new();
    }

    public override ValueTask StartAsync(CancellationToken cancellationToken)
    {
        _runAsync = RunAsync(_tokenSource.Token);
        return default;
    }

    public override async ValueTask StopAsync(CancellationToken cancellationToken)
    {
        _tokenSource.Cancel();
        try
        {
            await _runAsync!;
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        var day = TimeSpan.FromDays(1);
        await Task.Delay(day - DateTimeOffset.UtcNow.TimeOfDay + TimeSpan.FromHours(5), cancellationToken);
        using PeriodicTimer timer = new(day);
        while (true)
        {
            await using (var context = Provider.GetRequiredService<DataContext>())
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
                        await Client.Rest.SendMessageAsync(Config.DailyReputationReactions.ChannelId, new()
                        {
                            Embeds = new EmbedProperties[]
                            {
                                new()
                                {
                                    Description = stringBuilder.ToString(),
                                    Color = Config.EmbedColor,
                                },
                            },
                        });
                        stringBuilder.Clear();
                    }
                    stringBuilder.AppendLine(line);
                }
                if (stringBuilder.Length != 0)
                {
                    await Client.Rest.SendMessageAsync(Config.DailyReputationReactions.ChannelId, new()
                    {
                        Embeds = new EmbedProperties[]
                        {
                            new()
                            {
                                Description = stringBuilder.ToString(),
                                Color = Config.EmbedColor,
                            },
                        },
                    });
                }
                await context.SaveChangesAsync(default);
                await transaction.CommitAsync(default);
            }
            await timer.WaitForNextTickAsync(cancellationToken);
        }
    }
}
