using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

public sealed class AppDbContextFactory(IConfiguration configuration, IDbIntentAccessor intentAccessor) : IAppDbContextFactory
{
    public IAppDbContext Create()
    {
        return intentAccessor.Intent == DbIntent.Read
            ? CreateReadDbcontext()
            : CreateWriteDbcontext();
    }

    public IAppDbContext CreateReadDbcontext()
    {
        var readConnectionString = configuration.GetConnectionString("Read") ?? throw new ArgumentException("Read connection string is not configured");
        return new ReadDbContext(readConnectionString, "Replica");
    }

    public IAppDbContext CreateWriteDbcontext()
    {
        var readWriteConnectionString = configuration.GetConnectionString("ReadWrite") ?? throw new ArgumentException("ReadWrite connection string is not configured");
        return new ReadWriteDbContext(readWriteConnectionString, "Master");
    }
}
