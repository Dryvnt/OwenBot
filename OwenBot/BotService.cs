using System.Text.RegularExpressions;
using System.Threading.Channels;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OwenBot;

public partial class BotService(
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<BotService> logger,
    IHttpClientFactory httpClientFactory,
    OwenApi owenApi,
    DiscordClient discord)
    : BackgroundService
{
    private readonly Channel<DiscordMessage> _replyToQueue = Channel.CreateUnbounded<DiscordMessage>();

    private static bool ShouldReply(BaseDiscordClient sender, MessageCreateEventArgs e)
    {
        // Early check to prevent make dang sure we prevent infinite loops!
        if (e.Author.IsBot) return false;

        if (e.MentionedUsers.Contains(sender.CurrentUser)) return true;

        return MagicWordRegex().IsMatch(e.Message.Content);
    }

    private async Task ReplyWow(DiscordMessage message, CancellationToken stoppingToken)
    {
        var wow = await owenApi.GetRandomAsync(stoppingToken);
        using var httpClient = httpClientFactory.CreateClient();
        await using var videoStream = await httpClient.GetStreamAsync(wow.VideoLinkCollection.Video360P, stoppingToken);
        await message.RespondAsync(msg => { msg.AddFile("wow.mp4", videoStream); });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        discord.MessageCreated += async (s, e) =>
        {
            if (ShouldReply(s, e)) await _replyToQueue.Writer.WriteAsync(e.Message, stoppingToken);
        };
        discord.SocketOpened += (_, _) =>
        {
            logger.LogInformation("Connected!");
            return Task.CompletedTask;
        };
        discord.SocketClosed += (_, args) =>
        {
            logger.LogInformation("Socket closed ({}, {})", args.CloseCode, args.CloseMessage);
            return Task.CompletedTask;
        };
        discord.SocketErrored += (_, args) =>
        {
            logger.LogError(args.Exception, "Socket error!");
            return Task.CompletedTask;
        };

        try
        {
            await discord.ConnectAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await foreach (var message in _replyToQueue.Reader.ReadAllAsync(stoppingToken))
                    await ReplyWow(message, stoppingToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error during execute loop");
        }
        finally
        {
            // If for some reason we make it here,
            // but it's not because the application is being stopped,
            // we make sure the application _will_ be stopped.
            logger.LogInformation("Message receive loop interrupted. Forcing application shutdown");
            hostApplicationLifetime.StopApplication();
        }
    }

    [GeneratedRegex("wow|car key", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MagicWordRegex();
}
