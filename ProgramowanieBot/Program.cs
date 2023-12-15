using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.Interactions;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.Interactions;

using ProgramowanieBot;
using ProgramowanieBot.Data;
using ProgramowanieBot.Helpers;

var builder = Host.CreateDefaultBuilder(args)
    .UseDiscordGateway(options =>
    {
        options.Configuration = new()
        {
            Intents = GatewayIntents.Guilds | GatewayIntents.GuildUsers | GatewayIntents.GuildPresences | GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessageTyping,
        };
    })
    .UseApplicationCommandService<SlashCommandInteraction, SlashCommandContext>(OptionsHelper.ConfigureApplicationCommandService)
    .UseApplicationCommandService<UserCommandInteraction, UserCommandContext>(OptionsHelper.ConfigureApplicationCommandService)
    .UseApplicationCommandService<MessageCommandInteraction, MessageCommandContext>(OptionsHelper.ConfigureApplicationCommandService)
    .UseInteractionService<ButtonInteraction, ButtonInteractionContext>(OptionsHelper.ConfigureInteractionService)
    .UseInteractionService<StringMenuInteraction, StringMenuInteractionContext>(OptionsHelper.ConfigureInteractionService)
    .UseInteractionService<UserMenuInteraction, UserMenuInteractionContext>(OptionsHelper.ConfigureInteractionService)
    .UseInteractionService<ModalSubmitInteraction, ModalSubmitInteractionContext>(OptionsHelper.ConfigureInteractionService)
    .ConfigureServices(services =>
    {
        services.AddSingleton(Configuration.Create())
                .AddDbContext<DataContext>((provider, optionsBuilder) =>
                {
                    var connection = provider.GetRequiredService<Configuration>().Database.CreateConnectionString();
                    optionsBuilder.UseNpgsql(connection);
                }, ServiceLifetime.Transient, ServiceLifetime.Singleton)
                .AddHttpClient()
                .AddGatewayEventHandlers(typeof(Program).Assembly)
                .AddHostedService<DailyReputationBackgroundService>();
    });

var host = builder.Build()
    .AddModules(typeof(Program).Assembly)
    .UseGatewayEventHandlers();

await host.RunAsync();
