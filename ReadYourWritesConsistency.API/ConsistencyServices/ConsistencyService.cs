using ReadYourWritesConsistency.API.Persistence;

namespace ReadYourWritesConsistency.API.ConsistencyServices;

public class ConsistencyService(ConsistencyContext context, ConsistencyStore store, IAppDbContextFactory appDbContextFactory) : IConsistencyService
{
    public async Task<bool> IsReplicaConsistent()
    {
        var getTimestampTasks = context.Resources
            .Select(x => store.GetTimestampAsync(x.Item1, x.Item2));

        var response = await Task.WhenAll(getTimestampTasks);

        if (response ==  null || response.Length == 0)
        {
            return false;
        }

        var timeStamp = response
            .Where(x => x != null)
            .Select(x => new { Value = x, DateTime = DateTime.Parse(x ?? string.Empty) })
            .OrderByDescending(x => x.DateTime)
            .Select(x => x.Value)
            .FirstOrDefault();

        if (string.IsNullOrEmpty(timeStamp))
        {
            return false;
        }

        var result = await appDbContextFactory
            .CreateReadDbContext()
            .QueryStoredProcAsync<int>("[dbo].[Consistency_IsCaughtUp]", new { Timestamp = timeStamp });

        if (!result.IsSuccess)
        {
            return false;
        }

        return result.Value?.FirstOrDefault() == 1;
    }
}
