using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

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


