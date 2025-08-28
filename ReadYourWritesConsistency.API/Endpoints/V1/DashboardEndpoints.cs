using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Services;

namespace ReadYourWritesConsistency.API.Endpoints.V1;

public static class DashboardEndpoints
{
	public static RouteGroupBuilder MapV1Dashboard(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/v1/dashboard");
		
		group.MapGet("", async (ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory) =>
		{
			int requestingUserId = currentUser.UserId;
			var db = dbFactory.Create();
			var result = await db.QueryStoredProcAsync<DashboardProjectDto>(
				"[dbo].[Projects_GetDashboard]",
				new { RequestingUserId = requestingUserId });
			return result.IsSuccess 
				? Results.Ok(Result<IEnumerable<DashboardProjectDto>>.Success(result.Value!, result.DbSource)) 
				: Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
		});
		
		return group;
	}
}


