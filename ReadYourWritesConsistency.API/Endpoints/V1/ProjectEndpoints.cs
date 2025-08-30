using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Persistence;
using ReadYourWritesConsistency.API.Services;

namespace ReadYourWritesConsistency.API.Endpoints.V1;

public static class ProjectEndpoints
{
    public static RouteGroupBuilder MapV1Projects(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects");

        group.MapGet("/{projectId:int}", GetProjectAsync);

        group.MapGet("/{projectId:int}/tasks", GetProjectTasksAsync);

        return group;
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
}


