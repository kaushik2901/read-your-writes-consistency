using ReadYourWritesConsistency.API.ConsistencyServices;
using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

public sealed class AppDbContextFactory(IConfiguration configuration, IDbIntentAccessor intentAccessor, ConsistencyContext consistencyContext) : IAppDbContextFactory
{
    public IAppDbContext Create()
    {
        return intentAccessor.Intent == DbIntent.Read
            ? CreateReadDbContext()
            : CreateWriteDbContext();
    }

    public IAppDbContext CreateReadDbContext()
    {
        var readConnectionString = configuration.GetConnectionString("Read") ?? throw new ArgumentException("Read connection string is not configured");
        return new ReadDbContext(readConnectionString, "Replica");
    }

    public IAppDbContext CreateWriteDbContext()
    {
        var readWriteConnectionString = configuration.GetConnectionString("ReadWrite") ?? throw new ArgumentException("ReadWrite connection string is not configured");
        return new ReadWriteDbContext(consistencyContext, readWriteConnectionString, "Master");
    }
}
