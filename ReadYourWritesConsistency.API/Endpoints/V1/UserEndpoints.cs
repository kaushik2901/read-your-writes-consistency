using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Services;

namespace ReadYourWritesConsistency.API.Endpoints.V1;

public static class UserEndpoints
{
	public static RouteGroupBuilder MapV1Users(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/v1/users");

		group.MapGet("", async (IAppDbContextFactory dbFactory) =>
		{
			var db = dbFactory.Create();
			var result = await db.QueryStoredProcAsync<UserDto>(
				"[dbo].[Users_GetAll]");
			return result.IsSuccess 
				? Results.Ok(Result<IEnumerable<UserDto>>.Success(result.Value!, result.DbSource)) 
				: Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
		});

		return group;
	}
}


