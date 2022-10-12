using System.Reflection;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ProgramowanieBot;

internal class BotService : IHostedService
{
    public GatewayClient Client { get; }

    private readonly ILogger _logger;
    private readonly Snowflake _clientId;
    private readonly Snowflake _forumChannelId;
    private readonly IReadOnlyDictionary<Snowflake, Snowflake> _forumTagsRoles;
    private readonly string _forumPostStartMessage;
    private readonly ApplicationCommandService<SlashCommandContext> _applicationCommandService;

    public BotService(ILogger<BotService> logger, IConfiguration configuration)
    {
        _logger = logger;

        Token token = new(TokenType.Bot, configuration.GetRequiredSection("ProgramowanieBotToken").Value);
        _clientId = token.Id;
        Client = new(token, new()
        {
            Intents = GatewayIntent.Guilds | GatewayIntent.GuildUsers | GatewayIntent.GuildPresences,
        });
        Client.Log += message =>
        {
            _logger.Log(message.Severity switch
            {
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Error => LogLevel.Error,
                _ => LogLevel.Warning
            }, "{message} {description}", message.Message, message.Description ?? string.Empty);
            return default;
        };

        _forumChannelId = new(configuration.GetRequiredSection("ForumChannelId").Value);
        _forumTagsRoles = configuration.GetRequiredSection("ForumTagsRoles").Get<IReadOnlyDictionary<string, string>>().ToDictionary(x => new Snowflake(x.Key), x => new Snowflake(x.Value));
        _forumPostStartMessage = configuration["ForumPostStartMessage"];
        Client.GuildThreadCreate += HandleThreadCreateAsync;

        _applicationCommandService = new();
        _applicationCommandService.AddModules(Assembly.GetEntryAssembly()!);
        Client.InteractionCreate += HandleInteractionAsync;
    }

    private async ValueTask HandleInteractionAsync(Interaction interaction)
    {
        try
        {
            await (interaction switch
            {
                SlashCommandInteraction slashCommandInteraction => _applicationCommandService.ExecuteAsync(new(slashCommandInteraction, Client)),
                _ => throw new("Invalid interaction."),
            });
        }
        catch (Exception ex)
        {
            try
            {
                await interaction.SendResponseAsync(InteractionCallback.ChannelMessageWithSource(new()
                {
                    Content = $"<a:nie:881595378070343710> {ex.Message}",
                    Flags = MessageFlags.Ephemeral,
                }));
            }
            catch
            {
            }
        }
    }

    private async ValueTask HandleThreadCreateAsync(GuildThreadCreateEventArgs args)
    {
        if (args.NewlyCreated && args.Thread is PublicGuildThread thread && thread.ParentId == _forumChannelId)
        {
            var appliedTags = thread.AppliedTags;
            if (appliedTags != null)
            {
                Snowflake roleId = default;
                if (!appliedTags.Any(t => _forumTagsRoles.TryGetValue(t, out roleId)))
                    return;

                RestMessage message;
                MessageProperties messageProperties = $"{_forumPostStartMessage}\nPing: <@&{roleId}>";
                while (true)
                {
                    try
                    {
                        message = await thread.SendMessageAsync(messageProperties);
                        break;
                    }
                    catch (RestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        if ((await JsonDocument.ParseAsync(await ex.ResponseContent.ReadAsStreamAsync())).RootElement.GetProperty("code").GetInt32() != 40058)
                            throw;
                    }
                }

                if (message.Flags.HasFlag(MessageFlags.FailedToMentionSomeRolesInThread) && Client.Guilds.TryGetValue(args.Thread.GuildId, out var guild))
                {
                    StringBuilder stringBuilder = new(2000, 2000);
                    List<Task> tasks = new(1);
                    foreach (var user in guild.Users.Values.Where(u => u.RoleIds.Contains(roleId)))
                    {
                        var mention = user.ToString();
                        if (stringBuilder.Length + mention.Length > 2000)
                        {
                            tasks.Add(SendAndDeleteMessageAsync());
                            stringBuilder.Clear();
                        }
                        stringBuilder.Append(mention);
                    }
                    if (stringBuilder.Length != 0)
                        tasks.Add(SendAndDeleteMessageAsync());

                    await Task.WhenAll(tasks);

                    async Task SendAndDeleteMessageAsync()
                    {
                        var message = await thread.SendMessageAsync(stringBuilder.ToString());
                        try
                        {
                            await message.DeleteAsync();
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering application commands");
        var list = await _applicationCommandService.CreateCommandsAsync(Client.Rest, _clientId);
        _logger.LogInformation("{count} command(s) successfully registered", list.Count);
        await Client.StartAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Client.CloseAsync();
}
