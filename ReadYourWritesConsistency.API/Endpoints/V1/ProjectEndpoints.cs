using System.Data;
using Dapper;
using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Persistence;
using ReadYourWritesConsistency.API.Services;
using static Dapper.SqlMapper;

namespace ReadYourWritesConsistency.API.Endpoints.V1;

public static class ProjectEndpoints
{
    public static RouteGroupBuilder MapV1Projects(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects");

        group.MapPost("", GetProjectsAsync);

        group.MapGet("/{projectId:int}", GetProjectAsync);

        group.MapGet("/{projectId:int}/tasks", GetProjectTasksAsync);

        group.MapPut("/{projectId:int}", UpdateProjectAsync);

        group.MapDelete("/{projectId:int}", DeleteProjectAsync);

        return group;
    }

    private static async Task<IResult> GetProjectsAsync(CreateProjectRequest req, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory)
    {
        var parameters = new DynamicParameters();
        parameters.Add("RequestingUserId", currentUser.UserId, DbType.Int32);
        parameters.Add("Name", req.Name, DbType.String);
        parameters.Add("MemberUserIds", CreateUserIdList(req.MemberUserIds));

        var result = await dbFactory
            .Create()
            .ExecuteStoredProcAsync(
                "[dbo].[Project_Create_V1]",
                parameters
            );

        return result.IsSuccess
            ? Results.Ok(Result.Success(result.DbSource))
            : Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
    }

    private static async Task<IResult> GetProjectAsync(int projectId, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory)
    {
        var result = await dbFactory
            .Create()
            .QueryMultiResultStoredProcAsync<ProjectMetaDataDto, ProjectMemberDto>(
                "[dbo].[Project_Get_V1]",
                new { RequestingUserId = currentUser.UserId, ProjectId = projectId }
            );

        if (!result.IsSuccess)
        {
            Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
        }

        var projectMetaDataDto = result.Value.Item1.FirstOrDefault();
        if (projectMetaDataDto == null || projectMetaDataDto.Id == 0)
            return Results.Ok(Result.Failure("Project not found", "Replica"));

        var projectMembers = result.Value.Item2.ToList();
        var dto = new ProjectDto(projectMetaDataDto, projectMembers);

        return Results.Ok(Result<ProjectDto>.Success(dto, "Replica"));
    }

    private static async Task<IResult> GetProjectTasksAsync(int projectId, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory)
    {
        var result = await dbFactory
            .Create()
            .QueryStoredProcAsync<TaskListItemDto>(
                "[dbo].[Project_GetTasks_V1]",
                new { RequestingUserId = currentUser.UserId, ProjectId = projectId }
            );

        return result.IsSuccess
            ? Results.Ok(Result<IEnumerable<TaskListItemDto>>.Success(result.Value!, result.DbSource))
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
            .ExecuteStoredProcAsync(
                "[dbo].[Project_Update_V1]",
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
            .ExecuteStoredProcAsync(
                "[dbo].[Project_Delete_V1]",
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


