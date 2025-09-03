using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

public class ReadDbContext(string connectionString, string dbSource) : IAppDbContext
{
    protected readonly string _connectionString = connectionString;
    protected readonly string _dbSource = dbSource;

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public Task<Result> ExecuteStoredProcAsync(string storedProc, object? parameters = null)
    {
        return Task.FromResult(Result.Failure("Can not execute write based stored procedures on read-only database.", _dbSource));
    }

    public Task<Result> ExecuteStoredProcWithTimestampCaptureAsync(string storedProc, object? parameters = null)
    {
        return Task.FromResult(Result.Failure("Can not execute write based stored procedures on read-only database.", _dbSource));
    }

    public async Task<Result<(IEnumerable<A>, IEnumerable<B>)>> QueryMultiResultStoredProcAsync<A, B>(string storedProc, object? parameters = null)
    {
        try
        {
            using var conn = CreateConnection();

            await using var multi = await conn.QueryMultipleAsync(
                storedProc,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var a = await multi.ReadAsync<A>();
            var b = await multi.ReadAsync<B>();

            return Result<(IEnumerable<A>, IEnumerable<B>)>.Success((a, b), _dbSource);
        }
        catch (SqlException ex)
        {
            return Result<(IEnumerable<A>, IEnumerable<B>)>.Failure(ex.Message, _dbSource);
        }
        catch (Exception ex)
        {
            return Result<(IEnumerable<A>, IEnumerable<B>)>.Failure(ex.Message, _dbSource);
        }
    }

    public async Task<Result<IEnumerable<T>>> QueryStoredProcAsync<T>(string storedProc, object? parameters = null)
    {
        try
        {
            using var conn = CreateConnection();
            var rows = await conn.QueryAsync<T>(storedProc, parameters, commandType: CommandType.StoredProcedure);
            return Result<IEnumerable<T>>.Success(rows, _dbSource);
        }
        catch (SqlException ex)
        {
            return Result<IEnumerable<T>>.Failure(ex.Message, _dbSource);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<T>>.Failure(ex.Message, _dbSource);
        }
    }
}

