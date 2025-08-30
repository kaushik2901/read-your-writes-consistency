namespace ReadYourWritesConsistency.API.Models;

public sealed record DashboardProjectDto
(
    int Id,
    string Name,
    int NewCount,
    int ActiveCount,
    int BlockedCount,
    DateTime LastUpdatedAtUtc
);

public sealed record ProjectDto
(
    int Id,
    string Name,
    int CreatedByUserId,
    DateTime LastUpdatedAtUtc,
    IReadOnlyList<ProjectMemberDto> Members
);

public sealed record ProjectMemberDto(int UserId, string DisplayName);

public sealed record TaskListItemDto
(
    int Id,
    string Name,
    string Status,
    string? UserName,
    DateTime LastModifiedAtUtc
);

public sealed record UserDto(int Id, string UserName, string DisplayName);
public sealed record CreateProjectRequest(string Name, IReadOnlyList<int> MemberUserIds);
public sealed record UpdateProjectRequest(string? Name, IReadOnlyList<int>? MemberUserIds);
public sealed record CreateTaskRequest(int ProjectId, string Name, int? AssignedUserId, string Status);
public sealed record UpdateTaskRequest(int TaskId, string? Name, string? Status, int? AssignedUserId);


