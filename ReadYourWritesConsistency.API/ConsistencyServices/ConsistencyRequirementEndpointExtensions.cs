namespace ReadYourWritesConsistency.API.ConsistencyServices;

public class ConsistencyRequirement
{
    public required string ResourceType { get; set; }
    public required string ResourceIdParameter { get; set; }
    public List<string> ParentResourceTypes { get; set; } = [];
}

public static class ConsistencyRequirementEndpointExtensions
{
    public static RouteHandlerBuilder RequiresConsistency(
        this RouteHandlerBuilder builder,
        string resourceType,
        string resourceIdParameter,
        params string[] parentResourceTypes)
    {
        return builder.WithMetadata(new ConsistencyRequirement
        {
            ResourceType = resourceType,
            ResourceIdParameter = resourceIdParameter,
            ParentResourceTypes = [.. parentResourceTypes]
        });
    }
}
