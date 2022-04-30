using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OwenBot;

public class BotService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BotService> _logger;
    private readonly OwenApi _owen;
    private readonly DiscordClient _discord;

    private static readonly IReadOnlySet<string> MagicWords = new HashSet<string>
    {
        "wow",
        "car key",
    };

    public BotService(ILogger<BotService> logger, IHttpClientFactory httpClientFactory, OwenApi owenApi, DiscordClient discord)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _owen = owenApi;
        _discord = discord;
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
        var httpClient = _httpClientFactory.CreateClient();
        var videoStream = await httpClient.GetStreamAsync(wow.VideoLinkCollection.Video360p);
        await e.Message.RespondAsync(msg =>
        {
            msg.WithFile("wow.mp4", videoStream);
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        _discord.MessageCreated += OnMessageCreated;
        _discord.SocketOpened += (s, e) =>
        {
            _logger.LogInformation("Connected!");
            return Task.CompletedTask;
        };
        _discord.SocketClosed += (sender, args) =>
        {
            cts.Cancel();
            return Task.CompletedTask;
        };
        _discord.SocketErrored += (sender, args) =>
        {
            cts.Cancel();
            return Task.CompletedTask;
        };
            await _discord.ConnectAsync();
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
