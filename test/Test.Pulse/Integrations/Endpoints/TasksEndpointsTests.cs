using System.Net.Http.Json;
using Pulse.Api.Contracts;
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
        using var client  = _factory.CreateClient();
        var now = _factory.DateTimeService.UtcNow();
        var create = new
        {
            Title = "Write integration tests",
            Description = "Cover all routes",
            DueDateUtc = now.AddHours(2)
        };
       
        var createResponse = await client.PostAsJsonAsync("/api/tasks", create);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();
      
        var get = await client.GetAsync($"/api/tasks/{id}");
        var response = await get.Content.ReadFromJsonAsync<TaskResponse>();
        
        response.ShouldNotBeNull();
        response.Id.ShouldBe(id);
        response.Title.ShouldBe(create.Title);
        response.Status.ShouldBe(TaskStatusDto.New);
        response.DueDateUtc.ShouldBe(create.DueDateUtc);
    }

    // [Test]
    // public async Task List_Should_Return_Page_With_Items()
    // {
    //     using var factory = CreateFactory();
    //     using var client  = factory.CreateClient();
    //
    //     for (var i = 0; i < 3; i++)
    //     {
    //         var req = new { Title = $"T{i}", Description = $"D{i}", DueDateUtc = DateTime.UtcNow.AddHours(1 + i) };
    //         var r = await client.PostAsJsonAsync("/api/tasks", req);
    //         r.EnsureSuccessStatusCode();
    //     }
    //
    //     var list = await client.GetAsync("/api/tasks?page=1&size=10");
    //     list.StatusCode.Should().Be(HttpStatusCode.OK);
    //
    //     var page = await list.Content.ReadFromJsonAsync<PagedResponse<TaskResponse>>();
    //     page.Should().NotBeNull();
    //     page!.Items.Should().NotBeEmpty();
    //     page.Total.Should().BeGreaterOrEqualTo(3);
    // }
    //
    // [Test]
    // public async Task Update_ChangeStatus_Delete_Flow()
    // {
    //     using var factory = CreateFactory();
    //     using var client  = factory.CreateClient();
    //
    //     var create = await client.PostAsJsonAsync("/api/tasks", new {
    //         Title = "Flow",
    //         Description = "Update → Status → Delete",
    //         DueDateUtc = DateTime.UtcNow.AddHours(3)
    //     });
    //     var id = await create.Content.ReadFromJsonAsync<Guid>();
    //
    //     var update = await client.PutAsJsonAsync($"/api/tasks/{id}", new {
    //         Title = "Flow (updated)",
    //         Description = "Updated",
    //         DueDateUtc = DateTime.UtcNow.AddHours(4)
    //     });
    //     update.StatusCode.Should().Be(HttpStatusCode.NoContent);
    //
    //     var patch = await client.PatchAsJsonAsync($"/api/tasks/{id}/status", new {
    //         Status = "Completed"
    //     });
    //     patch.StatusCode.Should().Be(HttpStatusCode.NoContent);
    //
    //     var del = await client.DeleteAsync($"/api/tasks/{id}");
    //     del.StatusCode.Should().Be(HttpStatusCode.NoContent);
    //
    //     var get = await client.GetAsync($"/api/tasks/{id}");
    //     get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    // }
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeAsync();
    }
}