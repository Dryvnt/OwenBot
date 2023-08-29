using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OwenBot;

using var host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices(
        (builder, services) =>
        {
            services.AddLogging();
            services.AddHttpClient();

            services.AddSingleton(
                sp => new DiscordClient(
                    new DiscordConfiguration
                    {
                        Token = builder.Configuration.GetRequiredSection("OwenBot:DiscordApiToken").Get<string>(),
                        TokenType = TokenType.Bot,
                        Intents = DiscordIntents.DirectMessages | DiscordIntents.GuildMessages |
                                  DiscordIntents.MessageContents,
                        LoggerFactory = sp.GetRequiredService<ILoggerFactory>(),
                        AutoReconnect = true,
                    }
                )
            );
            services.AddSingleton<OwenApi>();

            services.AddHostedService<BotService>();
        }
    )
    .Build();

await host.RunAsync();