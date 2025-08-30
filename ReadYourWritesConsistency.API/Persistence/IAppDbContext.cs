using System.Data;
using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

public interface IAppDbContext
{
    IDbConnection CreateConnection();
    Task<Result<IEnumerable<T>>> QueryStoredProcAsync<T>(string storedProc, object? parameters = null);
    Task<Result<(IEnumerable<A>, IEnumerable<B>)>> QueryMultiResultStoredProcAsync<A, B>(string storedProc, object? parameters = null);
    Task<Result> ExecuteStoredProcAsync(string storedProc, object? parameters = null);
}
