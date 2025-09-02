using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Persistence;

namespace ReadYourWritesConsistency.API.Endpoints.V1;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapV1Users(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users");

        group.MapGet("", GetUsersAsync);

        return group;
    }

    public static async Task<IResult> GetUsersAsync(IAppDbContextFactory dbFactory)
    {
        var result = await dbFactory
            .Create()
            .QueryStoredProcAsync<UserDto>("[dbo].[Users_GetAll_V1]");

        return result.IsSuccess
            ? Results.Ok(Result<IEnumerable<UserDto>>.Success(result.Value!, result.DbSource))
            : Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
    }
}


