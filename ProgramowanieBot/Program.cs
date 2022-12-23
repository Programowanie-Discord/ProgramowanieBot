using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetCord.Gateway;

using ProgramowanieBot;
using ProgramowanieBot.Handlers;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddSingleton(ConfigService.Create())
            .AddSingleton<TokenService>()
            .AddSingleton<HttpClient>()
            .AddSingleton<GatewayClient>(provider => new(provider.GetRequiredService<TokenService>().Token, new()
            {
                Intents = GatewayIntent.Guilds | GatewayIntent.GuildUsers | GatewayIntent.GuildPresences | GatewayIntent.GuildMessages | GatewayIntent.MessageContent,
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
                       .AddSingleton<IHandler, GuildThreadCreateHandler>();
    }
}
