namespace ReadYourWritesConsistency.API.Endpoints.V2;

public static class V2Scaffold
{
	public static void MapV2Scaffold(this IEndpointRouteBuilder app)
	{
		var v2 = app.MapGroup("/api/v2");
		v2.MapFallback(() => Results.StatusCode(StatusCodes.Status501NotImplemented));
	}
}


