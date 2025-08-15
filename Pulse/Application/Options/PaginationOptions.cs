using System.ComponentModel.DataAnnotations;
using Pulse.Domain.Models;

namespace Pulse.Application.Options;

public sealed class PaginationOptions
{
    [Range(1, 10000)] public int DefaultPage { get; init; } = 1;
    [Range(1, 10000)] public int DefaultSize { get; init; } = 20;
    [Range(1, 10000)] public int MaxSize     { get; init; } = 100;
    public PulseTaskSort DefaultSort { get; init; } = PulseTaskSort.DueDateAsc;
}