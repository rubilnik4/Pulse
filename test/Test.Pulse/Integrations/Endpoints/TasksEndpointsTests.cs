namespace Test.Pulse.Integrations;

[TestFixture]
public sealed class TasksEndpointsTests
{
    private CustomWebApplicationFactory CreateFactory() => 
        new CustomWebApplicationFactory(TestDatabase.ConnectionString);

    [Test]
    public async Task Create_Then_Get_Should_Return_Created_And_Entity()
    {
        using var factory = CreateFactory();
        using var client  = factory.CreateClient();

        var create = new
        {
            Title = "Write integration tests",
            Description = "Cover all routes",
            DueDateUtc = DateTime.UtcNow.AddHours(2)
        };

        var createResponse = await client.PostAsJsonAsync("/api/tasks", create);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBeEmpty();

        var get = await client.GetAsync($"/api/tasks/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await get.Content.ReadFromJsonAsync<TaskResponse>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(id);
        dto.Title.Should().Be("Write integration tests");
    }

    [Test]
    public async Task List_Should_Return_Page_With_Items()
    {
        using var factory = CreateFactory();
        using var client  = factory.CreateClient();

        for (var i = 0; i < 3; i++)
        {
            var req = new { Title = $"T{i}", Description = $"D{i}", DueDateUtc = DateTime.UtcNow.AddHours(1 + i) };
            var r = await client.PostAsJsonAsync("/api/tasks", req);
            r.EnsureSuccessStatusCode();
        }

        var list = await client.GetAsync("/api/tasks?page=1&size=10");
        list.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await list.Content.ReadFromJsonAsync<PagedResponse<TaskResponse>>();
        page.Should().NotBeNull();
        page!.Items.Should().NotBeEmpty();
        page.Total.Should().BeGreaterOrEqualTo(3);
    }

    [Test]
    public async Task Update_ChangeStatus_Delete_Flow()
    {
        using var factory = CreateFactory();
        using var client  = factory.CreateClient();

        var create = await client.PostAsJsonAsync("/api/tasks", new {
            Title = "Flow",
            Description = "Update → Status → Delete",
            DueDateUtc = DateTime.UtcNow.AddHours(3)
        });
        var id = await create.Content.ReadFromJsonAsync<Guid>();

        var update = await client.PutAsJsonAsync($"/api/tasks/{id}", new {
            Title = "Flow (updated)",
            Description = "Updated",
            DueDateUtc = DateTime.UtcNow.AddHours(4)
        });
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var patch = await client.PatchAsJsonAsync($"/api/tasks/{id}/status", new {
            Status = "Completed"
        });
        patch.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var del = await client.DeleteAsync($"/api/tasks/{id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await client.GetAsync($"/api/tasks/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}