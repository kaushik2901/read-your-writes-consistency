namespace ReadYourWritesConsistency.API.Endpoints.V2;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapV2Dashboard(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/dashboard");

        group.MapGet("", V1.DashboardEndpoints.DashboardHandlerAsync);

        return group;
    }
}


