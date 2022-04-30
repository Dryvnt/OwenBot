using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OwenApiClient;
using OwenBot;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging();
        services.AddHostedService<BotService>();
        services.AddSingleton<IOwenApiClient, HttpOwen>();
    })
    .Build();

await host.RunAsync();
