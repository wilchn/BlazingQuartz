using System;
using Quartz;

namespace BlazingQuartz.Core.Test;

public class TestJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return Task.CompletedTask;
    }
}

public class TestJobWithDelay : IJob
{
    private readonly TimeSpan _delay;

    public TestJobWithDelay(TimeSpan delay)
    {
        _delay = delay;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await Task.Delay(_delay);

        await Task.CompletedTask;
    }
}
