using System.ComponentModel.DataAnnotations;

namespace Pulse.Api.Contracts.Requests;

public sealed record CreateTaskRequest(
    [property: Required] string Title,
    [property: Required] string Description,
    [property: Required] DateTimeOffset DueDateUtc
);