using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ProgramowanieBot;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddHostedService<BotService>();
});
var host = builder.Build();
await host.RunAsync();
