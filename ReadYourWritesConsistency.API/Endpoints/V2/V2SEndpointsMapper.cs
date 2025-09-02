namespace ReadYourWritesConsistency.API.Endpoints.V2;

public static class V2SEndpointsMapper
{
    public static RouteGroupBuilder MapV2Endpoints(this IEndpointRouteBuilder app)
    {
        var v2 = app.MapGroup("/v2");

        v2.MapV2Dashboard();
        v2.MapV2Projects();
        v2.MapV2Tasks();
        v2.MapV2Users();

        return v2;
    }
}
