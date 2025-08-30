namespace ReadYourWritesConsistency.API.Models;

public interface IDbIntentAccessor
{
    DbIntent Intent { get; set; }
}
