namespace ReadYourWritesConsistency.API.ConsistencyServices;

public class ConsistencyContext
{
    public List<(string, string)> Resources { get; set; } = [];
    public string? Timestamp { get; set; } = null;
}
