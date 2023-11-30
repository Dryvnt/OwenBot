using System.Text.Json;

namespace OwenBot;

public class OwenApi(IHttpClientFactory httpClientFactory)
{
    private static readonly Uri BaseAddress = new("https://owen-wilson-wow-api.onrender.com/wows/");

    public async Task<Wow> GetRandomAsync(CancellationToken stoppingToken = default)
    {
        var client = httpClientFactory.CreateClient();
        var responseStream = await client.GetStreamAsync(new Uri(BaseAddress, "random"), stoppingToken);
        return await JsonSerializer.DeserializeAsyncEnumerable<Wow>(responseStream, cancellationToken: stoppingToken)
                   .FirstOrDefaultAsync(stoppingToken) ??
               throw new InvalidOperationException("Could not deserialize api response from a successful request?");
    }
}
