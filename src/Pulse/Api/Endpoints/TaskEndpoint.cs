using Pulse.Api.Contracts.Requests;
using Pulse.Api.Contracts.Responses;
using Pulse.Api.Validation;

namespace Pulse.Api.Endpoints;

public static class TasksEndpoints
{
    public static RouteGroupBuilder GetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/tasks")
            .WithTags("Tasks");

        group.MapPost("/", TaskHandlers.Create)
            .AddEndpointFilter(new ValidateRequest<CreateTaskRequest>())
            .Produces<TaskResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithName("CreateTask");

        group.MapGet("/{id:guid}", TaskHandlers.GetById)
            .Produces<TaskResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetTaskById");

        group.MapGet("/", TaskHandlers.List)
            .Produces<PagedResponse<TaskResponse>>()
            .WithName("ListTasks");

        group.MapPut("/{id:guid}", TaskHandlers.Update)
            .AddEndpointFilter(new ValidateRequest<UpdateTaskRequest>())
            .Produces<TaskResponse>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdateTask");

        group.MapPatch("/{id:guid}/status", TaskHandlers.ChangeStatus)
            .AddEndpointFilter(new ValidateRequest<UpdateTaskStatusRequest>())
            .Produces<TaskResponse>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .WithName("ChangeTaskStatus");

        group.MapDelete("/{id:guid}", TaskHandlers.Delete)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteTask");

        return group;
    }
}