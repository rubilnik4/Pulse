using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using Pulse.Application.Commands;
using Pulse.Application.Options;
using Pulse.Application.Pagination;
using Pulse.Application.Queries;
using Pulse.Domain.Models;
using Pulse.Infrastructure.Errors;
using Pulse.Infrastructure.Repositories;

namespace Pulse.Application.Services;

public sealed class TaskService(
    ITaskRepository repository, 
    IDateTimeService dateTimeService, 
    IOptions<PaginationOptions> paginationOptions,
    ILogger<ITaskService> logger): ITaskService
{
    public Task<Result<Guid, IAppError>> Create(CreateTaskCommand command)
    {
        logger.LogInformation("Creating new task with title '{Title}'", command.Title);
        
        var taskItem = command.NewTask(dateTimeService.UtcNow());
        return repository.Insert(taskItem);
    }
  
    public async Task<Result<TaskItem, IAppError>> Get(Guid id)
    {
        logger.LogInformation("Getting task {TaskId}", id);
        
        var result = await repository.Get(id);

        if (result.IsFailure)
            return Result.Failure<TaskItem, IAppError>(result.Error);

        return result.Value.HasValue
            ? Result.Success<TaskItem, IAppError>(result.Value.Value)
            : Result.Failure<TaskItem, IAppError>(new NotFoundError(id.ToString(), "Get database task"));
    }
   
    public Task<Result<PageResult<TaskItem>, IAppError>> List(ListTasksQuery query)
    {
        logger.LogInformation("Listing tasks, Status={Status}, Page={Page}, Size={Size}, Sort={Sort}",
            query.Status, query.Page, query.Size, query.Sort);
        
        var page = query.Page > 0 
            ? query.Page 
            : paginationOptions.Value.DefaultPage;
        var size = query.Size > 0 
            ? Math.Min(query.Size, paginationOptions.Value.MaxSize) 
            : paginationOptions.Value.DefaultSize;
        return repository.List(query.Status, page, size, query.Sort);
    }
   
    public async Task<UnitResult<IAppError>> Update(UpdateTaskCommand command)
    {
        logger.LogInformation("Updating task {TaskId}", command.Id);
        
        var currentResult = await repository.Get(command.Id);
        if (currentResult.IsFailure)
            return currentResult;
        if (currentResult.Value.HasNoValue)
            return Result.Failure<TaskItem, IAppError>(new NotFoundError(command.Id.ToString(), "Get database task"));

        var currentTask = currentResult.Value.Value;
        var updatedTask = command.WithUpdate(currentTask, dateTimeService.UtcNow());
        return await repository.Update(updatedTask);
    }
   
    public async Task<UnitResult<IAppError>> ChangeStatus(Guid id, PulseTaskStatus status)
    {
        logger.LogInformation("Changing status for task {TaskId} -> {Status}", id, status);
        
        var updateResult = await repository.UpdateStatus(id, status, dateTimeService.UtcNow());
        if (updateResult.IsFailure)
            return updateResult.Error.ErrorType == DatabaseError.NotFoundErrorType
                ? UnitResult.Failure<IAppError>(new NotFoundError(id.ToString(), "Update status database task"))
                : updateResult;

        return updateResult;
    }
 
    public async Task<UnitResult<IAppError>> Delete(Guid id)
    {
        logger.LogInformation("Deleting task {TaskId}", id);
        
        var deletedResult = await repository.Delete(id);
        if (deletedResult.IsFailure)
            return deletedResult.Error.ErrorType == DatabaseError.NotFoundErrorType
                ? UnitResult.Failure<IAppError>(new NotFoundError(id.ToString(), "Delete database task"))
                : deletedResult;
        
        return deletedResult;
    }
}