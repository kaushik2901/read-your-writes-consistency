namespace ReadYourWritesConsistency.API.Endpoints.V2;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapV2Users(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users");

        group.MapGet("", V1.UserEndpoints.GetUsersAsync);

        return group;
    }
}


