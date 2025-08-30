namespace ReadYourWritesConsistency.API.Endpoints.V1;

public static class V1EndpointsMapper
{
    public static RouteGroupBuilder MapV1Endpoints(this IEndpointRouteBuilder builder)
    {
        var v1 = builder.MapGroup("/v1");

        v1.MapV1Dashboard();
        v1.MapV1Projects();
        v1.MapV1Tasks();
        v1.MapV1Users();

        return v1;
    }
}
