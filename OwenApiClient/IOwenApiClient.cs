namespace OwenApiClient;

public interface IOwenApiClient
{
    public Task<Wow> GetRandomAsync();
}