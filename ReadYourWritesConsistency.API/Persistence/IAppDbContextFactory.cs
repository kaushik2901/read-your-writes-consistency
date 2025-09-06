namespace ReadYourWritesConsistency.API.Persistence;

public interface IAppDbContextFactory
{
    IAppDbContext Create();
    IAppDbContext CreateReadDbContext();
    IAppDbContext CreateWriteDbContext();
}


