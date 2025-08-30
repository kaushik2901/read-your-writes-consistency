namespace ReadYourWritesConsistency.API.Models;

public sealed class DbIntentAccessor : IDbIntentAccessor
{
    public DbIntent Intent { get; set; } = DbIntent.Read;
}


