using System.ComponentModel.DataAnnotations;

namespace Pulse.Api.Contracts.Requests;

public sealed record UpdateTaskRequest(
    [property: Required] string Title,
    [property: Required] string Description,
    [property: Required] DateTimeOffset DueDateUtc,
    [property: Required, EnumDataType(typeof(TaskStatusDto))] TaskStatusDto Status
);