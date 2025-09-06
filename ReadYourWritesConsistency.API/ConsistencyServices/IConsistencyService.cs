namespace ReadYourWritesConsistency.API.ConsistencyServices;

public interface IConsistencyService
{
    Task<bool> IsReplicaConsistent();
}