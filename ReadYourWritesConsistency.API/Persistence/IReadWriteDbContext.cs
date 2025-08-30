using System.Data;
using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

public interface IReadWriteDbContext
{
    IDbConnection CreateConnection();
    Task<Result> ExecuteStoredProcAsync(string storedProc, object? parameters = null);
    Task<Result<IEnumerable<T>>> ExecuteStoredProcAsync<T>(string storedProc, object? parameters = null);
}


