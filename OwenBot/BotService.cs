using System.Text.RegularExpressions;
using System.Threading.Channels;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OwenBot;

public class BotService : BackgroundService
{
    private static readonly Regex MagicWordChecker = new(
        "wow|car key",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
    );

    private readonly DiscordClient _discord;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BotService> _logger;
    private readonly OwenApi _owen;
    private readonly Channel<DiscordMessage> _replyToQueue = Channel.CreateUnbounded<DiscordMessage>();

    public BotService(
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<BotService> logger,
        IHttpClientFactory httpClientFactory,
        OwenApi owenApi,
        DiscordClient discord
    )
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _owen = owenApi;
        _discord = discord;

        _discord.MessageCreated += async (s, e) =>
        {
            if (ShouldReply(s, e)) await _replyToQueue.Writer.WriteAsync(e.Message);
        };
        _discord.SocketOpened += (_, _) =>
        {
            _logger.LogInformation("Connected!");
            return Task.CompletedTask;
        };
        _discord.SocketClosed += (_, args) =>
        {
            _logger.LogInformation("Socket closed ({}, {})", args.CloseCode, args.CloseMessage);
            return Task.CompletedTask;
        };
        _discord.SocketErrored += (_, args) =>
        {
            _logger.LogInformation(args.Exception, "Socket error!");
            return Task.CompletedTask;
        };
    }

    private static bool ShouldReply(BaseDiscordClient sender, MessageCreateEventArgs e)
    {
        // Early check to prevent make dang sure we prevent infinite loops!
        if (e.Author.IsBot) return false;

        if (e.MentionedUsers.Contains(sender.CurrentUser)) return true;

        return MagicWordChecker.IsMatch(e.Message.Content);
    }

    private async Task ReplyWow(DiscordMessage message, CancellationToken stoppingToken)
    {
        var wow = await _owen.GetRandomAsync(stoppingToken);
        var httpClient = _httpClientFactory.CreateClient();
        var videoStream = await httpClient.GetStreamAsync(wow.VideoLinkCollection.Video360P, stoppingToken);
        await message.RespondAsync(msg => { msg.AddFile("wow.mp4", videoStream); });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _discord.ConnectAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await foreach (var message in _replyToQueue.Reader.ReadAllAsync(stoppingToken))
                        await ReplyWow(message, stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error during receive-reply loop");
                    throw;
                }
            }
        }
        finally
        {
            // If for some reason we make it here,
            // but it's not because the application is being stopped,
            // we make sure the application _will_ be stopped.
            _logger.LogInformation("Message receive loop interrupted. Forcing application shutdown");
            _hostApplicationLifetime.StopApplication();
        }
    }
}
