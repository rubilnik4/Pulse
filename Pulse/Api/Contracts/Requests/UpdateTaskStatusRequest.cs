using System.ComponentModel.DataAnnotations;

namespace Pulse.Api.Contracts.Requests;

public sealed record UpdateTaskStatusRequest(
    [property: Required, EnumDataType(typeof(TaskStatusDto))] TaskStatusDto Status
);