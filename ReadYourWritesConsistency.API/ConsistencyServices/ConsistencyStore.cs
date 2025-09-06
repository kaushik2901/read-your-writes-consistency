using StackExchange.Redis;

namespace ReadYourWritesConsistency.API.ConsistencyServices;

public class ConsistencyStore(IConnectionMultiplexer connectionMultiplexer)
{
    private readonly IDatabase _redisDatabase = connectionMultiplexer.GetDatabase();

    public async Task<string?> GetTimestampAsync(string resource, string id)
    {
        var key = $"{resource}-{id}";
        var timestamp = await _redisDatabase.StringGetAsync(key);
        return timestamp.HasValue ? timestamp.ToString() : null;
    }

    public async Task<bool> SetTimestampAsync(string resource, string id, string timeStamp)
    {
        var key = $"{resource}-{id}";
        var response = await _redisDatabase.StringSetAsync(key, timeStamp);
        return response;
    }
}
