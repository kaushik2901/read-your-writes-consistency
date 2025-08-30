using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

internal sealed class AppDbContextAdapter : IAppDbContext
{
    private readonly ReadDbContext? _read;
    private readonly ReadWriteDbContext? _write;

    public AppDbContextAdapter(ReadDbContext read)
    {
        _read = read;
    }

    public AppDbContextAdapter(ReadWriteDbContext write)
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


