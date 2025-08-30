using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.Persistence;

public interface IDbIntentAccessor
{
    DbIntent Intent { get; set; }
}


