using ReadYourWritesConsistency.API.Services;

namespace ReadYourWritesConsistency.API.Models;

public enum DbIntent
{
	Read,
	Write
}

public interface IDbIntentAccessor
{
	DbIntent Intent { get; set; }
}

public sealed class DbIntentAccessor : IDbIntentAccessor
{
	public DbIntent Intent { get; set; } = DbIntent.Read;
}

public interface IAppDbContextFactory
{
	IAppDbContext Create();
}

public sealed class AppDbContextFactory : IAppDbContextFactory
{
	private readonly IDbIntentAccessor _intentAccessor;
	private readonly IReadDbContext _read;
	private readonly IReadWriteDbContext _write;

	public AppDbContextFactory(IDbIntentAccessor intentAccessor, IReadDbContext read, IReadWriteDbContext write)
	{
		_intentAccessor = intentAccessor;
		_read = read;
		_write = write;
	}

	public IAppDbContext Create()
	{
		return _intentAccessor.Intent == DbIntent.Read
			? new AppDbContextAdapter(_read)
			: new AppDbContextAdapter(_write);
	}
}

internal sealed class AppDbContextAdapter : IAppDbContext
{
	private readonly IReadDbContext? _read;
	private readonly IReadWriteDbContext? _write;

	public AppDbContextAdapter(IReadDbContext read) 
	{ 
		_read = read;
	}
	
	public AppDbContextAdapter(IReadWriteDbContext write) 
	{ 
		_write = write;
	}

	public System.Data.IDbConnection CreateConnection()
	{
		return _read?.CreateConnection() ?? _write!.CreateConnection();
	}

	public async Task<Result<IEnumerable<T>>> QueryStoredProcAsync<T>(string storedProc, object? parameters = null)
	{
		if (_read != null) 
		{
			return await _read.ExecuteStoredProcAsync<T>(storedProc, parameters);
		}
		
		return await _write!.ExecuteStoredProcAsync<T>(storedProc, parameters);
	}

	public async Task<Result> ExecuteStoredProcAsync(string storedProc, object? parameters = null)
	{
		if (_write != null) 
		{
			return await _write.ExecuteStoredProcAsync(storedProc, parameters);
		}
		
		// Allow read contexts to execute when needed (no-op proc or read-only proc)
		return await _read!.ExecuteStoredProcAsync(storedProc, parameters);
	}
}


