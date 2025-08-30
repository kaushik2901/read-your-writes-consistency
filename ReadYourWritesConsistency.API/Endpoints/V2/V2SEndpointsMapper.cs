namespace ReadYourWritesConsistency.API.Endpoints.V2;

public static class V2SEndpointsMapper
{
    public static void MapV2Endpoints(this IEndpointRouteBuilder app)
    {
        var v2 = app.MapGroup("/v2");
        v2.MapFallback(() => Results.StatusCode(StatusCodes.Status501NotImplemented));
    }
}


