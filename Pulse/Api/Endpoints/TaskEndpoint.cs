using Pulse.Api.Contracts.Requests;
using Pulse.Api.Contracts.Responses;
using Pulse.Api.Mapping;
using Pulse.Api.Validation;
using Pulse.Application.Services;

namespace Pulse.Api.Endpoints;

using Microsoft.AspNetCore.Mvc;

public static class TasksEndpoints {
    public static RouteGroupBuilder GetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/tasks")
            .WithTags("Tasks");
        
        group.MapPost("/", async ([FromBody] CreateTaskRequest request, ITaskService service) =>
        {
            var result = await service.Create(request.ToCommand());
            if (result.IsFailure) 
                return ProblemFromError(result.Error);
            
            var response = result.Value.ToResponse();
            return Results.Created($"/api/tasks/{response.Id}", response);
        })
        .AddEndpointFilter(new ValidateRequest<CreateTaskRequest>())
        .Produces<TaskResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .WithName("CreateTask");
       
        group.MapGet("/", async ([AsParameters] PagingQuery query, ITaskService service) =>
        {
            var pages = await service.List(query.ToQuery());
            return Results.Ok(pages.ToPagedResponse());
        })
        .Produces<PagedResponse<TaskResponse>>()
        .WithName("ListTasks");
      
        group.MapGet("/{id:guid}", async (Guid id, ITaskService service) =>
        {
            var task = await service.Get(id);
            return task.HasValue
                ? Results.Ok(task.Value.ToResponse())
                : Results.NotFound();
        })
        .Produces<TaskResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .WithName("GetTaskById");
       
        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateTaskRequest request, ITaskService service) =>
        {
            var result = await service.Update(request.ToCommand(id));
            return result.IsSuccess 
                ? Results.Ok(result.Value.ToResponse())
                : ProblemFromError(result.Error);
        })
        .AddEndpointFilter(new ValidateRequest<UpdateTaskRequest>())
        .Produces<TaskResponse>()
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound)
        .WithName("UpdateTask");
        
        group.MapPatch("/{id:guid}/status", async (Guid id, [FromBody] UpdateTaskStatusRequest req, ITaskService service) =>
        {
            var result = await service.ChangeStatus(id, req.Status.ToDomain());
            return result.IsSuccess 
                ? Results.Ok(result.Value.ToResponse()) 
                : ProblemFromError(result.Error);
        })
        .AddEndpointFilter(new ValidateRequest<UpdateTaskStatusRequest>())
        .Produces<TaskResponse>()
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound)
        .WithName("ChangeTaskStatus");

        // Delete
        group.MapDelete("/{id:guid}", async (Guid id, ITaskService service) =>
        {
            var result = await service.Delete(id);
            return result.IsSuccess 
                ? Results.NoContent() 
                : ProblemFromError(result.Error);
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("DeleteTask");

        return group;
    }

    private static IResult ProblemFromError(string error)
    {
        if (error.Equals("NotFound", StringComparison.OrdinalIgnoreCase))
            return Results.NotFound();
        if (error.StartsWith("Validation:", StringComparison.OrdinalIgnoreCase))
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                {"", new[] { error.Replace("Validation:", string.Empty).Trim() }}
            });
        return Results.BadRequest(new ProblemDetails { Title = "Operation failed", Detail = error });
    }
}