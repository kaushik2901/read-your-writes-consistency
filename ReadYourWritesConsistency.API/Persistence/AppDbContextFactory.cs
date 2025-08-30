using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

public sealed class AppDbContextFactory(IDbIntentAccessor intentAccessor, ReadDbContext read, ReadWriteDbContext write) : IAppDbContextFactory
{
    public IAppDbContext Create()
    {
        return intentAccessor.Intent == DbIntent.Read
            ? new AppDbContextAdapter(read)
            : new AppDbContextAdapter(write);
    }
}


