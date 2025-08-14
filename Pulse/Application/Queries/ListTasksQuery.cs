using Pulse.Domain.Models;

namespace Pulse.Application.Queries;

public sealed record ListTasksQuery(
    PulseTaskStatus? Status, 
    int Page, 
    int Size, 
    PulseTaskSort Sort
);