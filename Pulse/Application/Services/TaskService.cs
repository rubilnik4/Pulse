using CSharpFunctionalExtensions;
using Pulse.Application.Commands;
using Pulse.Application.Pagination;
using Pulse.Application.Queries;
using Pulse.Domain.Models;
using Pulse.Infrastructure.Repositories;

namespace Pulse.Application.Services;

public sealed class TaskService(
    ITaskRepository repository, 
    IDateTimeService dateTimeService) 
    : ITaskService
{
    public async Task<Result<TaskItem>> Create(CreateTaskCommand command)
    {
        var taskItem = CreateTaskCommand.NewTask(command, dateTimeService.UtcNow());

        var ok = await repository.Insert(taskItem);
        return ok 
            ? Result.Success(taskItem) 
            : Result.Failure<TaskItem>("CreateFailed");
    }
   
    public async Task<PageResult<TaskItem>> List(ListTasksQuery query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var size = query.Size <= 0 ? 20 : Math.Min(query.Size, 100);
        var offset = (page - 1) * size;
       
        var (items, total) = await repository.List(query.Status, offset, size, query.Sort);

        return new PageResult<TaskItem>
        (
            Page: page,
            Size: size,
            Total: total,
            Items: items
        );
    }
  
    public Task<Maybe<TaskItem>> Get(Guid id) => 
        repository.Get(id);
   
    public async Task<Result<TaskItem>> Update(UpdateTaskCommand command)
    {
        var currentItem = await repository.Get(command.Id);
        if (currentItem.HasNoValue)
            return Result.Failure<TaskItem>("NotFound");
       
        var updatedItem = UpdateTaskCommand.WithUpdate(command, currentItem.Value, dateTimeService.UtcNow());

        var ok = await repository.Update(updatedItem);
        return ok 
            ? Result.Success(updatedItem) 
            : Result.Failure<TaskItem>("UpdateFailed");
    }
   
    public async Task<Result> ChangeStatus(Guid id, PulseTaskStatus status)
    {
        var current = await repository.Get(id);
        if (current.HasNoValue)
            return Result.Failure<TaskItem>("NotFound");
  
        var ok = await repository.UpdateStatus(id, status, dateTimeService.UtcNow());
        if (!ok)
            return Result.Failure<TaskItem>("UpdateFailed");

        return Result.Success();
    }
 
    public async Task<Result> Delete(Guid id)
    {
        var ok = await repository.Delete(id);
        return ok ? Result.Success() : Result.Failure("NotFound");
    }
}