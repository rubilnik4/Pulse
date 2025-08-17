using Microsoft.Extensions.DependencyInjection;
using Pulse.Application.Commands;
using Pulse.Application.Services;
using Pulse.Domain.Models;
using Shouldly;
using Test.Pulse.Integrations.Startup;

namespace Test.Pulse.Integrations.Workers;

[TestFixture]
public sealed class OverdueSweepTests
{
    private CustomWebApplicationFactory _factory = null!;
    private IServiceScope _scope = null!;
    private ITaskService _tasks = null!;
    private FakeDateTimeService _clock = null!; 

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _factory = await CustomWebApplicationFactory.StartNewAsync();
    }

    [SetUp]
    public void Setup()
    {
        _scope = _factory.Services.CreateScope();
        _tasks = _scope.ServiceProvider.GetRequiredService<ITaskService>();
        _clock = _factory.DateTimeService;
        _clock.Set(new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc));
    }

    [Test]
    public async Task MarkOverdue_Should_Update_2_Of_3()
    {
        var now = _clock.UtcNow();

        var t1 = await _tasks.Create(new CreateTaskCommand("Task-1", "test-1", now.AddMinutes(10)));
        var t2 = await _tasks.Create(new CreateTaskCommand("Task-2", "test-2", now.AddMinutes(20)));
        var t3 = await _tasks.Create(new CreateTaskCommand("Task-3", "test-3", now.AddMinutes(90)));

        t1.IsSuccess.ShouldBeTrue();
        t2.IsSuccess.ShouldBeTrue();
        t3.IsSuccess.ShouldBeTrue();

        var id1 = t1.Value;
        var id2 = t2.Value;
        var id3 = t3.Value;

        _clock.Advance(TimeSpan.FromMinutes(30));

        var sweep = await _tasks.MarkOverdue();
        sweep.IsSuccess.ShouldBeTrue();
        sweep.Value.ShouldBe(2, "two tasks should be in Overdue status");

        var g1 = await _tasks.Get(id1);
        g1.IsSuccess.ShouldBeTrue();
        g1.Value.Status.ShouldBe(PulseTaskStatus.Overdue);

        var g2 = await _tasks.Get(id2);
        g2.IsSuccess.ShouldBeTrue();
        g2.Value.Status.ShouldBe(PulseTaskStatus.Overdue);

        var g3 = await _tasks.Get(id3);
        g3.IsSuccess.ShouldBeTrue();
        g3.Value.Status.ShouldBe(PulseTaskStatus.New);
    }

    [TearDown]
    public void TearDown()
    {
        _scope.Dispose();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeAsync();
    }
}