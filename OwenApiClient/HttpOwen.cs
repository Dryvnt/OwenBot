using System.Text.Json;

namespace OwenApiClient;

public class HttpOwen : IOwenApiClient
{
    private readonly HttpClient _httpClient;

    public HttpOwen()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://owen-wilson-wow-api.herokuapp.com/wows/");
    }

    public async Task<Wow> GetRandomAsync()
    {
        var responseStream = await _httpClient.GetStreamAsync("random");
        var wows = await JsonSerializer.DeserializeAsync<List<Wow>>(responseStream) ??
                   throw new NotImplementedException();

        return wows.First();
    }
}
