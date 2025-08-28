using System.Data;
using Dapper;
using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Services;

namespace ReadYourWritesConsistency.API.Endpoints.V1;

public static class ProjectWriteEndpoints
{
	public static RouteGroupBuilder MapV1ProjectWrites(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/v1/projects");

		group.MapPost("", async (CreateProjectRequest req, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory) =>
		{
			int requestingUserId = currentUser.UserId;

			// Create table-valued parameter for UserIdList
			var tvp = new DataTable();
			tvp.Columns.Add("UserId", typeof(int));
			if (req.MemberUserIds != null)
			{
				foreach (var id in req.MemberUserIds.Distinct())
				{
					tvp.Rows.Add(id);
				}
			}

			var parameters = new Dapper.DynamicParameters();
			parameters.Add("RequestingUserId", requestingUserId, DbType.Int32);
			parameters.Add("Name", req.Name, DbType.String);
			parameters.Add("MemberUserIds", tvp.AsTableValuedParameter("dbo.UserIdList"));

			var db = dbFactory.Create();
			var result = await db.ExecuteStoredProcAsync(
				"[dbo].[Project_Create]",
				parameters);
			return result.IsSuccess 
				? Results.Ok(Result.Success(result.DbSource)) 
				: Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
		});

		group.MapPut("/{projectId:int}", async (int projectId, UpdateProjectRequest req, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory) =>
		{
			int requestingUserId = currentUser.UserId;

			DataTable? tvp = null;
			if (req.MemberUserIds != null)
			{
				tvp = new DataTable();
				tvp.Columns.Add("UserId", typeof(int));
				foreach (var id in req.MemberUserIds.Distinct())
				{
					tvp.Rows.Add(id);
				}
			}

			var parameters = new Dapper.DynamicParameters();
			parameters.Add("RequestingUserId", requestingUserId, DbType.Int32);
			parameters.Add("ProjectId", projectId, DbType.Int32);
			parameters.Add("Name", req.Name, DbType.String);
			if (tvp != null)
			{
				parameters.Add("MemberUserIds", tvp.AsTableValuedParameter("dbo.UserIdList"));
			}
			else
			{
				parameters.Add("MemberUserIds", null);
			}

			var db = dbFactory.Create();
			var result = await db.ExecuteStoredProcAsync(
				"[dbo].[Project_Update]",
				parameters);
			return result.IsSuccess 
				? Results.Ok(Result.Success(result.DbSource)) 
				: Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
		});

		group.MapDelete("/{projectId:int}", async (int projectId, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory) =>
		{
			int requestingUserId = currentUser.UserId;
			var db = dbFactory.Create();
			var result = await db.ExecuteStoredProcAsync(
				"[dbo].[Project_Delete]",
				new { RequestingUserId = requestingUserId, ProjectId = projectId });
			return result.IsSuccess 
				? Results.Ok(Result.Success(result.DbSource)) 
				: Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
		});

		return group;
	}
}


