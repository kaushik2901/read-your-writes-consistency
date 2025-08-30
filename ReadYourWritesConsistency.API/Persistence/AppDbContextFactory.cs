using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

public sealed class AppDbContextFactory(IConfiguration configuration, IDbIntentAccessor intentAccessor) : IAppDbContextFactory
{
    public IAppDbContext Create()
    {
        var readConnectionString = configuration.GetConnectionString("Read") ?? throw new ArgumentException("Read connection string is not configured");
        var readWriteConnectionString = configuration.GetConnectionString("ReadWrite") ?? throw new ArgumentException("ReadWrite connection string is not configured");

        return intentAccessor.Intent == DbIntent.Read
            ? new ReadDbContext(readConnectionString, "Replica")
            : new ReadWriteDbContext(readWriteConnectionString, "Master");
    }
}
