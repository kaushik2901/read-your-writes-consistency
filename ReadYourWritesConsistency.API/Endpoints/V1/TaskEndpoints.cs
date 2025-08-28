using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Services;

namespace ReadYourWritesConsistency.API.Endpoints.V1;

public static class TaskEndpoints
{
	public static RouteGroupBuilder MapV1Tasks(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/v1/tasks");

		group.MapPost("", async (CreateTaskRequest req, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory) =>
		{
			int requestingUserId = currentUser.UserId;
			var db = dbFactory.Create();
			var result = await db.ExecuteStoredProcAsync(
				"[dbo].[Task_Create]",
				new
				{
					RequestingUserId = requestingUserId,
                    req.ProjectId,
                    req.Name,
                    req.AssignedUserId,
                    req.Status
				});
			return result.IsSuccess 
				? Results.Ok(Result.Success(result.DbSource)) 
				: Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
		});

		group.MapPut("/{taskId:int}", async (int taskId, UpdateTaskRequest req, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory) =>
		{
			int requestingUserId = currentUser.UserId;
			var db = dbFactory.Create();
			var result = await db.ExecuteStoredProcAsync(
				"[dbo].[Task_Update]",
				new
				{
					RequestingUserId = requestingUserId,
					TaskId = taskId,
                    req.Name,
                    req.Status,
                    req.AssignedUserId
				});
			return result.IsSuccess 
				? Results.Ok(Result.Success(result.DbSource)) 
				: Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
		});

		group.MapDelete("/{taskId:int}", async (int taskId, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory) =>
		{
			int requestingUserId = currentUser.UserId;
			var db = dbFactory.Create();
			var result = await db.ExecuteStoredProcAsync(
				"[dbo].[Task_Delete]",
				new { RequestingUserId = requestingUserId, TaskId = taskId });
			return result.IsSuccess 
				? Results.Ok(Result.Success(result.DbSource)) 
				: Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
		});

		return group;
	}
}


