using System.Data;
using Dapper;
using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Persistence;
using ReadYourWritesConsistency.API.Services;
using static Dapper.SqlMapper;

namespace ReadYourWritesConsistency.API.Endpoints.V2;

public static class ProjectEndpoints
{
    public static RouteGroupBuilder MapV2Projects(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects");

        group.MapPost("", CreateProjectAsync);

        group.MapGet("/{projectId:int}", V1.ProjectEndpoints.GetProjectAsync);

        group.MapGet("/{projectId:int}/tasks", V1.ProjectEndpoints.GetProjectTasksAsync);

        group.MapPut("/{projectId:int}", UpdateProjectAsync);

        group.MapDelete("/{projectId:int}", DeleteProjectAsync);

        return group;
    }

    private static async Task<IResult> CreateProjectAsync(CreateProjectRequest req, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory)
    {
        var parameters = new DynamicParameters();
        parameters.Add("RequestingUserId", currentUser.UserId, DbType.Int32);
        parameters.Add("Name", req.Name, DbType.String);
        parameters.Add("MemberUserIds", CreateUserIdList(req.MemberUserIds));

        var result = await dbFactory
            .Create()
            .ExecuteStoredProcWithTimestampCaptureAsync(
                "[dbo].[Project_Create_V2]",
                parameters
            );

        return result.IsSuccess
            ? Results.Ok(Result.Success(result.DbSource))
            : Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
    }

    private static async Task<IResult> UpdateProjectAsync(int projectId, UpdateProjectRequest req, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory)
    {
        var parameters = new DynamicParameters();
        parameters.Add("RequestingUserId", currentUser.UserId, DbType.Int32);
        parameters.Add("ProjectId", projectId, DbType.Int32);
        parameters.Add("Name", req.Name, DbType.String);
        parameters.Add("MemberUserIds", CreateUserIdList(req.MemberUserIds));

        var result = await dbFactory
            .Create()
            .ExecuteStoredProcWithTimestampCaptureAsync(
                "[dbo].[Project_Update_V2]",
                parameters
            );

        return result.IsSuccess
            ? Results.Ok(Result.Success(result.DbSource))
            : Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
    }

    private static async Task<IResult> DeleteProjectAsync(int projectId, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory)
    {
        var result = await dbFactory
            .Create()
            .ExecuteStoredProcWithTimestampCaptureAsync(
                "[dbo].[Project_Delete_V2]",
                new { RequestingUserId = currentUser.UserId, ProjectId = projectId }
            );

        return result.IsSuccess
            ? Results.Ok(Result.Success(result.DbSource))
            : Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
    }

    private static ICustomQueryParameter? CreateUserIdList(IReadOnlyList<int>? memberUserIds)
    {
        var tvp = new DataTable();

        tvp.Columns.Add("UserId", typeof(int));

        if (memberUserIds != null)
        {
            foreach (var id in memberUserIds.Distinct())
            {
                tvp.Rows.Add(id);
            }
        }

        return tvp?.AsTableValuedParameter("dbo.UserIdList") ?? null;
    }
}


