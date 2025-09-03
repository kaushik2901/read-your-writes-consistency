namespace ReadYourWritesConsistency.API.Persistence;

public interface IAppDbContextFactory
{
    IAppDbContext Create();
    IAppDbContext CreateReadDbcontext();
    IAppDbContext CreateWriteDbcontext();
}


