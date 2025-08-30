using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

public sealed class ReadDbContext : IReadDbContext
{
    private readonly string _connectionString;

    public ReadDbContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Read") ?? throw new ArgumentNullException(_connectionString);
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<Result<IEnumerable<T>>> ExecuteStoredProcAsync<T>(string storedProc, object? parameters = null)
    {
        try
        {
            using IDbConnection conn = CreateConnection();
            var rows = await conn.QueryAsync<T>(storedProc, parameters, commandType: CommandType.StoredProcedure);
            return Result<IEnumerable<T>>.Success(rows, "Replica");
        }
        catch (SqlException ex)
        {
            return Result<IEnumerable<T>>.Failure(ex.Message, "Replica");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<T>>.Failure(ex.Message, "Replica");
        }
    }

    public async Task<Result> ExecuteStoredProcAsync(string storedProc, object? parameters = null)
    {
        try
        {
            using IDbConnection conn = CreateConnection();
            await conn.ExecuteAsync(storedProc, parameters, commandType: CommandType.StoredProcedure);
            return Result.Success("Replica");
        }
        catch (SqlException ex)
        {
            return Result.Failure(ex.Message, "Replica");
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, "Replica");
        }
    }
}

