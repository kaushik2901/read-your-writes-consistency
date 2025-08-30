using System.Data;
using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

public interface IReadDbContext
{
    IDbConnection CreateConnection();
    Task<Result<IEnumerable<T>>> ExecuteStoredProcAsync<T>(string storedProc, object? parameters = null);
    Task<Result> ExecuteStoredProcAsync(string storedProc, object? parameters = null);
}


