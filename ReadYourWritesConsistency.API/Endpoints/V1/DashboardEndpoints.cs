using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Persistence;
using ReadYourWritesConsistency.API.Services;

namespace ReadYourWritesConsistency.API.Endpoints.V1;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapV1Dashboard(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/dashboard");

        group.MapGet("", DashboardHandlerAsync);

        return group;
    }

    private static async Task<IResult> DashboardHandlerAsync(ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory)
    {
        var result = await dbFactory
            .Create()
            .QueryStoredProcAsync<DashboardProjectDto>(
                "[dbo].[Projects_GetDashboard_V1]",
                new { RequestingUserId = currentUser.UserId }
            );

        return result.IsSuccess
            ? Results.Ok(Result<IEnumerable<DashboardProjectDto>>.Success(result.Value!, result.DbSource))
            : Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
    }
}


