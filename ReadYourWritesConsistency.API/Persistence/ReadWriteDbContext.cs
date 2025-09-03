using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

public sealed class ReadWriteDbContext(string connectionString, string dbSource) : ReadDbContext(connectionString, dbSource), IAppDbContext
{
    public new IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public new async Task<Result> ExecuteStoredProcAsync(string storedProc, object? parameters = null)
    {
        try
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(storedProc, parameters, commandType: CommandType.StoredProcedure);
            return Result.Success(_dbSource);
        }
        catch (SqlException ex)
        {
            return Result.Failure(ex.Message, _dbSource);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, _dbSource);
        }
    }

    public new async Task<Result> ExecuteStoredProcWithTimestampCaptureAsync(string storedProc, object? parameters = null)
    {
        try
        {
            var response = await QueryStoredProcAsync<DateTime>(storedProc, parameters);
            if (!response.IsSuccess)
            {
                return Result.Failure(response.Error!, _dbSource);
            }

            Console.WriteLine(response.Value!.First());

            return Result.Success(_dbSource);
        }
        catch (SqlException ex)
        {
            return Result.Failure(ex.Message, _dbSource);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, _dbSource);
        }
    }
}


