using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OwenApiClient;

namespace OwenBot;

public class BotService : BackgroundService
{
    private readonly DiscordClient _discord;
    private readonly HttpClient _http;
    private readonly ILogger<BotService> _logger;
    private readonly IOwenApiClient _owen;

    public BotService(IConfiguration configuration, IOwenApiClient owenApiClient, ILogger<BotService> logger,
        ILoggerFactory loggerFactory)
    {
        _http = new HttpClient();
        _owen = owenApiClient;
        _logger = logger;
        var token = configuration["OwenBot:DiscordApiToken"] ?? throw new NotImplementedException();
        _discord = new DiscordClient(new DiscordConfiguration
        {
            Token = token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged,
            LoggerFactory = loggerFactory,
        });
    }

    private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (e.Author.IsBot)
            return;
        var isMentioned = e.MentionedUsers.Contains(sender.CurrentUser);
        var containsWow = e.Message.Content.ToLowerInvariant().Contains("wow");
        if (!(isMentioned || containsWow))
        {
            return;
        }

        var wow = await _owen.GetRandomAsync();
        var videoStream = await _http.GetStreamAsync(wow.VideoLinkCollection.Video360p);
        await e.Message.RespondAsync(msg =>
        {
            msg.WithFile("wow.mp4", videoStream);
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discord.MessageCreated += OnMessageCreated;
        _discord.SocketOpened += (s, e) =>
        {
            _logger.LogInformation("Connected!");
            return Task.CompletedTask;
        };
        _logger.LogInformation("Attempting connection");
        await _discord.ConnectAsync();
        _logger.LogInformation("Done attempting connection");
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
