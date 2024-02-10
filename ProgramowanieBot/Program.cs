using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddDiscordGateway(options =>
    {
        options.Configuration = new()
        {
            Intents = GatewayIntents.Guilds
                      | GatewayIntents.GuildUsers
                      | GatewayIntents.GuildPresences
                      | GatewayIntents.GuildMessages
                      | GatewayIntents.MessageContent
                      | GatewayIntents.GuildMessageReactions
                      | GatewayIntents.GuildMessageTyping,
        };
    })
    .AddApplicationCommandService<SlashCommandInteraction, SlashCommandContext>(OptionsHelper.ConfigureApplicationCommandService)
    .AddApplicationCommandService<UserCommandInteraction, UserCommandContext>(OptionsHelper.ConfigureApplicationCommandService)
    .AddApplicationCommandService<MessageCommandInteraction, MessageCommandContext>(OptionsHelper.ConfigureApplicationCommandService)
    .AddInteractionService<ButtonInteraction, ButtonInteractionContext>(OptionsHelper.ConfigureInteractionService)
    .AddInteractionService<StringMenuInteraction, StringMenuInteractionContext>(OptionsHelper.ConfigureInteractionService)
    .AddInteractionService<UserMenuInteraction, UserMenuInteractionContext>(OptionsHelper.ConfigureInteractionService)
    .AddInteractionService<ModalSubmitInteraction, ModalSubmitInteractionContext>(OptionsHelper.ConfigureInteractionService)
    .AddDbContext<DataContext>((provider, optionsBuilder) =>
    {
        var connection = provider.GetRequiredService<IOptions<Configuration>>().Value.Database.CreateConnectionString();
        optionsBuilder.UseNpgsql(connection);
    }, ServiceLifetime.Transient, ServiceLifetime.Singleton)
    .AddHttpClient()
    .AddGatewayEventHandlers(typeof(Program).Assembly)
    .AddHostedService<DailyReputationBackgroundService>()
    .AddOptions<Configuration>()
    .BindConfiguration(string.Empty);

var host = builder.Build()
    .AddModules(typeof(Program).Assembly)
    .UseGatewayEventHandlers();

await host.RunAsync();
