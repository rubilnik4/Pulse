using Microsoft.AspNetCore.Mvc;
using Pulse.Api.Contracts.Requests;
using Pulse.Api.Mapping;
using Pulse.Application.Services;

namespace Pulse.Api.Endpoints;

public static class TaskHandlers
{
    public static async Task<IResult> Create(
        [FromBody] CreateTaskRequest request, ITaskService service, ILogger<ITaskService> log)
    {
        log.LogInformation("CreateTask: title={Title}, dueDate={DueDateUtc}", request.Title, request.DueDateUtc);
        var result = await service.Create(request.ToCommand());
        return result.Match(id => Results.Created($"/api/tasks/{id}", id));
    }

    public static async Task<IResult> GetById(
        Guid id, ITaskService service, ILogger<ITaskService> log)
    {
        log.LogDebug("GetTaskById: {TaskId}", id);
        var result = await service.Get(id);
        return result.Match(taskItem => Results.Ok(taskItem.ToResponse()));
    }

    public static async Task<IResult> List(
        [AsParameters] PagingQuery query, ITaskService service, ILogger<ITaskService> log)
    {
        log.LogDebug("ListTasks: page={Page}, size={Size}, status={Status}, sort={Sort}",
            query.Page, query.Size, query.Status, query.Sort);

        var result = await service.List(query.ToQuery());
        return result.Match(page => Results.Ok(page.ToPagedResponse()));
    }

    public static async Task<IResult> Update(
        Guid id, [FromBody] UpdateTaskRequest request, ITaskService service, ILogger<ITaskService> log)
    {
        log.LogInformation("UpdateTask: {TaskId}", id);
        var result = await service.Update(request.ToCommand(id));
        return result.Match(Results.NoContent);
    }

    public static async Task<IResult> ChangeStatus(
        Guid id, [FromBody] UpdateTaskStatusRequest request, ITaskService service, ILogger<ITaskService> log)
    {
        log.LogInformation("ChangeTaskStatus: {TaskId} -> {Status}", id, request.Status);
        var result = await service.ChangeStatus(id, request.Status.ToDomain());
        return result.Match(Results.NoContent);
    }

    public static async Task<IResult> Delete(
        Guid id, ITaskService service, ILogger<ITaskService> log)
    {
        log.LogInformation("DeleteTask: {TaskId}", id);
        var result = await service.Delete(id);
        return result.Match(Results.NoContent);
    }
}