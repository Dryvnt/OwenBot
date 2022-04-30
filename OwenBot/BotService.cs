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

    private static readonly IReadOnlySet<string> MagicWords = new HashSet<string>
    {
        "wow",
        "car key",
    };

    public BotService(IConfiguration configuration, IOwenApiClient owenApiClient, ILogger<BotService> logger,
        ILoggerFactory loggerFactory)
    {
        _http = new HttpClient();
        _owen = owenApiClient;
        _logger = logger;
        var token = configuration.GetRequiredSection("OwenBot:DiscordApiToken").Get<string>();
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
        // Early check to prevent make dang sure we prevent infinite loops!
        if (e.Author.IsBot)
            return;

        if (e.MentionedUsers.Contains(sender.CurrentUser))
            await SayWow(e);

        var lowercaseMessage = e.Message.Content.ToLowerInvariant();
        if (MagicWords.Any(lowercaseMessage.Contains))
            await SayWow(e);

    }

    private async Task SayWow(MessageCreateEventArgs e)
    {
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
        await _discord.ConnectAsync();
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
