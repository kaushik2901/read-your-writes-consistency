namespace ReadYourWritesConsistency.API.Endpoints.V1;

public static class V1EndpointsMapper
{
    public static RouteGroupBuilder MapV1Endpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/v1");

        group.MapV1Dashboard();
        group.MapV1Projects();
        group.MapV1ProjectWrites();
        group.MapV1Tasks();
        group.MapV1Users();

        return group;
    }
}
