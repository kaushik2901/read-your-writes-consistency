using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

public sealed class ReadWriteDbContext : IReadWriteDbContext
{
    private readonly string _connectionString;

    public ReadWriteDbContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReadWrite") ?? throw new ArgumentNullException(_connectionString);
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<Result> ExecuteStoredProcAsync(string storedProc, object? parameters = null)
    {
        try
        {
            using IDbConnection conn = CreateConnection();
            await conn.ExecuteAsync(storedProc, parameters, commandType: CommandType.StoredProcedure);
            return Result.Success("Master");
        }
        catch (SqlException ex)
        {
            return Result.Failure(ex.Message, "Master");
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message, "Master");
        }
    }

    public async Task<Result<IEnumerable<T>>> ExecuteStoredProcAsync<T>(string storedProc, object? parameters = null)
    {
        try
        {
            using IDbConnection conn = CreateConnection();
            var rows = await conn.QueryAsync<T>(storedProc, parameters, commandType: CommandType.StoredProcedure);
            return Result<IEnumerable<T>>.Success(rows, "Master");
        }
        catch (SqlException ex)
        {
            return Result<IEnumerable<T>>.Failure(ex.Message, "Master");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<T>>.Failure(ex.Message, "Master");
        }
    }
}


