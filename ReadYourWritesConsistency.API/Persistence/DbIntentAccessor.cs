namespace ReadYourWritesConsistency.API.Persistence;

public sealed class DbIntentAccessor : IDbIntentAccessor
{
    public DbIntent Intent { get; set; } = DbIntent.Read;
}


