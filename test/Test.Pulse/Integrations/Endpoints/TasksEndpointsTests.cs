using System.Net;
using System.Net.Http.Json;
using Pulse.Api.Contracts;
using Pulse.Api.Contracts.Requests;
using Pulse.Api.Contracts.Responses;
using Shouldly;
using Test.Pulse.Integrations.Startup;

namespace Test.Pulse.Integrations.Endpoints;

[TestFixture]
public sealed class TasksEndpointsTests
{
    private CustomWebApplicationFactory _factory = null!;
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = await CustomWebApplicationFactory.StartNewAsync();
    }

    [Test]
    public async Task Create_Then_Get_Should_Return_Created_And_Entity()
    {
        using var client = _factory.CreateClient();
        var now = _factory.DateTimeService.UtcNow();

        var createRequest = TaskCreateRequest(now);
        var createResponse = await client.PostAsJsonAsync("/api/tasks", createRequest);
        createResponse.IsSuccessStatusCode.ShouldBeTrue();
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();
      
        var get = await client.GetAsync($"/api/tasks/{id}");
        var response = await get.Content.ReadFromJsonAsync<TaskResponse>();
        
        response.ShouldNotBeNull();
        response.Id.ShouldBe(id);
        response.Title.ShouldBe(createRequest.Title);
        response.Status.ShouldBe(TaskStatusDto.New);
        response.DueDateUtc.ShouldBe(createRequest.DueDateUtc);
    }

    [Test]
    public async Task List_Should_Return_Page_With_Items()
    {
        using var client = _factory.CreateClient();
        var now = _factory.DateTimeService.UtcNow();
        
        const int maxItems = 3;
        for (var i = 0; i < maxItems; i++)
        {
            var request = TaskCreateRequest(now);
            var response = await client.PostAsJsonAsync("/api/tasks", request);
            response.IsSuccessStatusCode.ShouldBeTrue();
        }
    
        var list = await client.GetAsync("/api/tasks?page=1&size=10");
        list.StatusCode.ShouldBe(HttpStatusCode.OK);
    
        var page = await list.Content.ReadFromJsonAsync<PagedResponse<TaskResponse>>();
        page.ShouldNotBeNull();
        page.Items.ShouldNotBeEmpty();
        page.Total.ShouldBeGreaterThanOrEqualTo(maxItems);
    }
    
    [Test]
    public async Task Update_Then_ChangeStatus_Should_Succeed()
    {
        using var client = _factory.CreateClient();
        var now = _factory.DateTimeService.UtcNow();
        
        var createRequest = TaskCreateRequest(now);
        var createResponse = await client.PostAsJsonAsync("/api/tasks", createRequest);
        createResponse.IsSuccessStatusCode.ShouldBeTrue();
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();
        id.ShouldNotBe(Guid.Empty);
        
        var updateDueDate = now.AddHours(4);
        var updateRequest = TaskUpdateRequest(updateDueDate);
        var updateResponse = await client.PutAsJsonAsync($"/api/tasks/{id}", updateRequest);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await client.GetAsync($"/api/tasks/{id}");
        var response = await get.Content.ReadFromJsonAsync<TaskResponse>();
        response.ShouldNotBeNull();
        response.Status.ShouldBe(updateRequest.Status);
        response.DueDateUtc.ShouldBe(updateRequest.DueDateUtc);
    }
    
    [Test]
    public async Task Patch_Then_ChangeStatus_Should_Succeed()
    {
        using var client = _factory.CreateClient();
        var now = _factory.DateTimeService.UtcNow();
        
        var createRequest = TaskCreateRequest(now);
        var createResponse = await client.PostAsJsonAsync("/api/tasks", createRequest);
        createResponse.IsSuccessStatusCode.ShouldBeTrue();
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();
        id.ShouldNotBe(Guid.Empty);
        
        var patchRequest = TaskUpdateStatusRequest();
        var patchResponse = await client.PatchAsJsonAsync($"/api/tasks/{id}/status", patchRequest);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await client.GetAsync($"/api/tasks/{id}");
        var response = await get.Content.ReadFromJsonAsync<TaskResponse>();
        response.ShouldNotBeNull();
        response.Status.ShouldBe(patchRequest.Status);
    }

    [Test]
    public async Task Delete_Should_Remove_Task()
    {
        using var client = _factory.CreateClient();
        var now = _factory.DateTimeService.UtcNow();

        var createRequest = TaskCreateRequest(now);
        var createResponse = await client.PostAsJsonAsync("/api/tasks", createRequest);
        createResponse.IsSuccessStatusCode.ShouldBeTrue();
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();
        id.ShouldNotBe(Guid.Empty);
       
        var deleteResponse = await client.DeleteAsync($"/api/tasks/{id}");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        
        var getResponse = await client.GetAsync($"/api/tasks/{id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeAsync();
    }
    
    private static CreateTaskRequest TaskCreateRequest(DateTime now) => new(
        Title: "Write integration tests create",
        Description: "Cover all routes",
        DueDateUtc: now.AddHours(2)
    );
    
    private static UpdateTaskRequest TaskUpdateRequest(DateTimeOffset dueDateUtc) => new(
        Title: "Write integration tests update",
        Description: "Cover all routes",
        DueDateUtc: dueDateUtc,
        Status: TaskStatusDto.InProgress
    );
    
    private static UpdateTaskStatusRequest TaskUpdateStatusRequest() => new(
        Status: TaskStatusDto.InProgress
    );
}