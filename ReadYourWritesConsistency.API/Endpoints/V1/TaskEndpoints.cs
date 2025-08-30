using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Persistence;
using ReadYourWritesConsistency.API.Services;

namespace ReadYourWritesConsistency.API.Endpoints.V1;

public static class TaskEndpoints
{
    public static RouteGroupBuilder MapV1Tasks(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tasks");

        group.MapPost("", CreateTaskAsync);

        group.MapPut("/{taskId:int}", UpdateTaskAsync);

        group.MapDelete("/{taskId:int}", DeleteTaskAsync);

        return group;
    }

    private static async Task<IResult> CreateTaskAsync(CreateTaskRequest req, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory)
    {
        var result = await dbFactory
            .Create()
            .ExecuteStoredProcAsync(
                "[dbo].[Task_Create]",
                new
                {
                    RequestingUserId = currentUser.UserId,
                    req.ProjectId,
                    req.Name,
                    req.AssignedUserId,
                    req.Status
                }
            );

        return result.IsSuccess
            ? Results.Ok(Result.Success(result.DbSource))
            : Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
    }

    private static async Task<IResult> UpdateTaskAsync(int taskId, UpdateTaskRequest req, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory)
    {
        var result = await dbFactory
            .Create()
            .ExecuteStoredProcAsync(
                "[dbo].[Task_Update]",
                new
                {
                    RequestingUserId = currentUser.UserId,
                    TaskId = taskId,
                    req.Name,
                    req.Status,
                    req.AssignedUserId
                }
            );

        return result.IsSuccess
            ? Results.Ok(Result.Success(result.DbSource))
            : Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
    }

    private static async Task<IResult> DeleteTaskAsync(int taskId, ICurrentUserAccessor currentUser, IAppDbContextFactory dbFactory)
    {
        var result = await dbFactory
            .Create()
            .ExecuteStoredProcAsync(
                "[dbo].[Task_Delete]",
                new { RequestingUserId = currentUser.UserId, TaskId = taskId }
            );

        return result.IsSuccess
            ? Results.Ok(Result.Success(result.DbSource))
            : Results.BadRequest(Result.Failure(result.Error!, result.DbSource));
    }
}


