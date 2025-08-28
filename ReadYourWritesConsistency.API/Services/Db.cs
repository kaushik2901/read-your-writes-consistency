using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Services;

public interface IAppDbContext
{
	IDbConnection CreateConnection();
	Task<Result<IEnumerable<T>>> QueryStoredProcAsync<T>(string storedProc, object? parameters = null);
	Task<Result> ExecuteStoredProcAsync(string storedProc, object? parameters = null);
}

public interface IReadDbContext
{
	IDbConnection CreateConnection();
	Task<Result<IEnumerable<T>>> ExecuteStoredProcAsync<T>(string storedProc, object? parameters = null);
	Task<Result> ExecuteStoredProcAsync(string storedProc, object? parameters = null);
}

public sealed class ReadDbContext : IReadDbContext
{
	private readonly string _connectionString;

	public ReadDbContext(string connectionString)
	{
		_connectionString = connectionString;
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

public interface IReadWriteDbContext
{
	IDbConnection CreateConnection();
	Task<Result> ExecuteStoredProcAsync(string storedProc, object? parameters = null);
	Task<Result<IEnumerable<T>>> ExecuteStoredProcAsync<T>(string storedProc, object? parameters = null);
}

public sealed class ReadWriteDbContext : IReadWriteDbContext
{
	private readonly string _connectionString;

	public ReadWriteDbContext(string connectionString)
	{
		_connectionString = connectionString;
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


