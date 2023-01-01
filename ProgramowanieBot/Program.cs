using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetCord.Gateway;

using ProgramowanieBot;
using ProgramowanieBot.Data;
using ProgramowanieBot.Handlers;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddSingleton(ConfigService.Create())
            .AddDbContext<DataContext>((provider, optionsBuilder) =>
            {
                var connection = provider.GetRequiredService<ConfigService>().Database.CreateConnectionString();
                optionsBuilder.UseNpgsql(connection);
            }, ServiceLifetime.Transient, ServiceLifetime.Singleton)
            .AddSingleton<TokenService>()
            .AddSingleton<HttpClient>()
            .AddSingleton<GatewayClient>(provider => new(provider.GetRequiredService<TokenService>().Token, new()
            {
                Intents = GatewayIntents.Guilds | GatewayIntents.GuildUsers | GatewayIntents.GuildPresences | GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessageTyping,
            }))
            .AddHandlers()
            .AddHostedService<BotService>();
});
var host = builder.Build();
await host.RunAsync();

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        return services.AddSingleton<IHandler, InteractionHandler>()
                       .AddSingleton<IHandler, MessageHandler>()
                       .AddSingleton<IHandler, GuildThreadCreateHandler>()
                       .AddSingleton<IHandler, ReactionHandler>()
                       .AddSingleton<IHandler, DailyReputationHandler>();
    }
}
