using System.Text.Json.Serialization;
using ReadYourWritesConsistency.API.Models;

namespace ReadYourWritesConsistency.API.JSONSerialization;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(CreateProjectRequest))]
[JsonSerializable(typeof(UpdateProjectRequest))]
[JsonSerializable(typeof(CreateTaskRequest))]
[JsonSerializable(typeof(UpdateTaskRequest))]
[JsonSerializable(typeof(DashboardProjectDto))]
[JsonSerializable(typeof(ProjectDto))]
[JsonSerializable(typeof(TaskListItemDto))]
[JsonSerializable(typeof(ProjectMemberDto))]
[JsonSerializable(typeof(ProjectMetaDataDto))]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(Result<IEnumerable<DashboardProjectDto>>))]
[JsonSerializable(typeof(Result<ProjectDto>))]
[JsonSerializable(typeof(Result<IEnumerable<TaskListItemDto>>))]
[JsonSerializable(typeof(Result<ProjectMemberDto>))]
[JsonSerializable(typeof(Result<ProjectMetaDataDto>))]
[JsonSerializable(typeof(Result<IEnumerable<UserDto>>))]
public partial class ExtendedJsonSerializationContext : JsonSerializerContext
{
}
