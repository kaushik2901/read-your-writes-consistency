using Dapper;
using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Services;

namespace ReadYourWritesConsistency.API.Endpoints.V1;

public static class ProjectEndpoints
{
	public static RouteGroupBuilder MapV1Projects(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/v1/projects");

		group.MapGet("/{projectId:int}", async (int projectId, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory) =>
		{
			int requestingUserId = currentUser.UserId;
			var db = dbFactory.Create();
			using var conn = db.CreateConnection();
			await using var multi = await conn.QueryMultipleAsync(
				"[dbo].[Project_Get]",
				new { RequestingUserId = requestingUserId, ProjectId = projectId },
				commandType: System.Data.CommandType.StoredProcedure);
			var (Id, Name, CreatedByUserId, LastUpdatedAtUtc) = await multi.ReadFirstOrDefaultAsync<(int Id, string Name, int CreatedByUserId, DateTime LastUpdatedAtUtc)>();
			if (Id == 0) return Results.Ok(Result.Failure("Project not found", "Replica"));
			var members = (await multi.ReadAsync<ProjectMemberDto>()).ToList();
			var dto = new ProjectDto(Id, Name, CreatedByUserId, LastUpdatedAtUtc, members);
			return Results.Ok(Result<ProjectDto>.Success(dto, "Replica"));
		});

		group.MapGet("/{projectId:int}/tasks", async (int projectId, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory) =>
		{
			int requestingUserId = currentUser.UserId;
			var db = dbFactory.Create();
			var result = await db.QueryStoredProcAsync<TaskListItemDto>(
				"[dbo].[Project_GetTasks]",
				new { RequestingUserId = requestingUserId, ProjectId = projectId });
			return result.IsSuccess 
				? Results.Ok(Result<IEnumerable<TaskListItemDto>>.Success(result.Value!, result.DbSource)) 
				: Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
		});

		return group;
	}
}


